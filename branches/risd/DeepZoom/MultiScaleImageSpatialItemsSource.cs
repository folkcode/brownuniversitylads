using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace DeepZoom
{
    /// <summary>
    /// A spatial items source that is able to find and cache visible image 
    /// tiles in a the screen. Used in conjuction with ZoomableCanvas.
    /// </summary>
    public class MultiScaleImageSpatialItemsSource :
        IList,
        ZoomableCanvas.ISpatialItemsSource
    {
        private const int CacheCapacity = 300;       // limit cache to 300 tiles
        private const int MinLevelForWpfBug = 18;
        private const int GapBetweenItemsForWpfBug = 500000;
        private readonly Dictionary<string, BitmapSource> _tileCache = new Dictionary<string, BitmapSource>();
        private readonly Queue<string> _cachedTiles = new Queue<string>(CacheCapacity);
        private readonly MultiScaleTileSource _tileSource;
        private CancellationTokenSource _currentCancellationTokenSource = new CancellationTokenSource();
        private int _dummyItem = -1;
        private static readonly object CacheLock = new object();

        public MultiScaleImageSpatialItemsSource(MultiScaleTileSource tileSource)
        {
            _tileSource = tileSource;
        }

        public void InvalidateSource()
        {
            if (ExtentChanged != null)
                ExtentChanged(this, EventArgs.Empty);
            if (QueryInvalidated != null)
                QueryInvalidated(this, EventArgs.Empty);
        }

        public void clearCache()
        {
            _tileCache.Clear();
            _cachedTiles.Clear();
        }

        #region ISpatialItemsSource members

        public Rect Extent
        {
            get
            {
                return new Rect(_tileSource.ImageSize);
            }
            
        }

        public IEnumerable<int> Query(Rect rectangle)
        {
            // Due to a bug in the WPF ItemsContainerGenerator, when item indexes
            // are too far from one another the ItemsContainerGenerator takes too
            // long to realize an item (the duration of this process is proportional
            // to the offset between the next item to realize and the closest realized
            // item). Because of that, every other 500000th item is preloaded to 
            // reduce this effect to a minimum.
            // The choice of this "gap" number and the minimum level to start 
            // preloading is a tradeoff between input responsivity and overall waiting
            // time before the level is rendered (while the dummy elements are being processed).
            // More dummy elements = better responsivity, but more waiting time before rendering level images.

            if (CurrentLevel > MinLevelForWpfBug)
            {
                var start = _tileSource.TilesAtLevel(MinLevelForWpfBug);
                var end = _tileSource.TilesAtLevel((CurrentLevel + 1).AtMost(_tileSource.ZoomLimitLevel));
                for (var i = start; i < end; i += GapBetweenItemsForWpfBug)
                {
                   _dummyItem = i;
                    yield return i;
                }
            }

            _dummyItem = -1;

            foreach (var tile in _tileSource.VisibleTilesUntilFill(rectangle, CurrentLevel))
                yield return _tileSource.GetTileIndex(tile);
        }

        public event EventHandler ExtentChanged;
        public event EventHandler QueryInvalidated;

        #endregion

        private int _currentLevel;
        public int CurrentLevel
        {
            get { return _currentLevel; }
            set
            {
                if (value == _currentLevel) return;

                // Cancel all download tasks
                _currentCancellationTokenSource.Cancel();
                _currentCancellationTokenSource = new CancellationTokenSource();

                _currentLevel = value;
            }
        }

        public object this[int i]
        {
            get
            {
                if (_dummyItem == i)
                    return null;

                var tile = _tileSource.TileFromIndex(i);
                var tileId = tile.ToString();

                //if (_tileCache.ContainsKey(tileId))
                //    return new VisualTile(tile, _tileSource, _tileCache[tileId]);

                var tileVm = new VisualTile(tile, _tileSource);
                
                var imageSource = _tileSource.GetTileLayers(tile.Level, tile.Column, tile.Row);

                var uri = imageSource as Uri;
                if (uri != null)
                {
                    // Capture closure
                    var source = ImageLoader.LoadImage(uri);
                    if (source != null)
                    {
                        source = CacheTile(tileId, source);
                        tileVm.Source = source;
                    }
                    /*var token = _currentCancellationTokenSource.Token;
                    Task.Factory
                        .StartNew(() =>
                        {
                            var source = ImageLoader.LoadImage(uri);
                            if (source != null)
                                source = CacheTile(tileId, source);
                            return source;
                        }, token, TaskCreationOptions.None, TaskScheduler.Default)
                        .ContinueWith(t =>
                        {
                            if (t.Result != null)
                            {
                                tileVm.Source = t.Result;
                            }
                        }, TaskContinuationOptions.OnlyOnRanToCompletion);*/
                }
                else
                {
                    var stream = imageSource as Stream;
                    if (stream != null)
                    {
                        var source = new BitmapImage();
                        source.BeginInit();
                        source.CacheOption = BitmapCacheOption.OnLoad;
                        source.StreamSource = stream;
                        source.EndInit();

                        var src = CacheTile(tileId,(source));
                        tileVm.Source = src;
                    }
                    else return null;
                }

                return tileVm;
            }
            set { }
        }

        private BitmapSource CacheTile(string tileId, BitmapSource source)
        {
            if (_tileCache.ContainsKey(tileId))
                return _tileCache[tileId];

            lock (CacheLock)
            {
                if (_cachedTiles.Count >= CacheCapacity)
                {
                    _tileCache.Remove(_cachedTiles.Dequeue());
                }
                _cachedTiles.Enqueue(tileId);
                _tileCache.Add(tileId, source);
            }
            return source;

           
        }

        #region Irrelevant IList Members

        int IList.Add(object value)
        {
            return 0;
        }

        void IList.Clear()
        {
        }

        bool IList.Contains(object value)
        {
            return false;
        }

        int IList.IndexOf(object value)
        {
            return 0;
        }

        void IList.Insert(int index, object value)
        {
        }

        void IList.Remove(object value)
        {
        }

        void IList.RemoveAt(int index)
        {
        }

        void ICollection.CopyTo(Array array, int index)
        {
        }

        bool IList.IsFixedSize
        {
            get { return false; }
        }

        bool IList.IsReadOnly
        {
            get { return true; }
        }

        bool ICollection.IsSynchronized
        {
            get { return false; }
        }

        object ICollection.SyncRoot
        {
            get { return null; }
        }

        int ICollection.Count
        {
            get { return int.MaxValue; }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            yield break;
        }

        #endregion
    }
}

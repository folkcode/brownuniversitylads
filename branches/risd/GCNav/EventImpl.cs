using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Windows.Media;

namespace GCNav
{
    public class EventImpl
    {
        XmlTextReader _reader;
        List<Event> _events;
        double _distanceBetweenTicks, _recHeight;

        public EventImpl(string url, double distanceBetweenTicks, double recHeight) //need to pass it an xml file
        {
            _reader = new XmlTextReader(url);
            _events = new List<Event>();
            _distanceBetweenTicks = distanceBetweenTicks;
            _recHeight = recHeight;
            this.readAll();
            this.setWidths(); //sets widths based on the length of event
            sortEvents(_events);
            colorEvents(_events);
        }

        /// <summary>
        /// reads the event information from an XML file with attributes name, start, end, location, and description
        /// </summary>
        public void readAll()
        {
            Event myEvent;
            while (_reader.Read())
            {
                myEvent = new Event(300, _recHeight); //arbitrary width to be changed later based on the time span of the event
                switch (_reader.NodeType)
                {
                    case XmlNodeType.Element:

                        while (_reader.MoveToNextAttribute())
                        {
                            if (_reader.Name == "name")
                            {
                                myEvent.Event_Name = _reader.Value;
                            }
                            else if (_reader.Name == "start")
                            {
                                myEvent.Start = Convert.ToInt32(_reader.Value);
                            }
                            else if (_reader.Name == "end")
                            {
                                myEvent.End = Convert.ToInt32(_reader.Value);
                            }
                            else if (_reader.Name == "location")
                            {
                                myEvent.Location = _reader.Value;
                            }
                            else if (_reader.Name == "description")
                            {
                                myEvent.Description = _reader.Value;
                            }
                        }
                        if (myEvent.Event_Name != "")
                        {
                            _events.Add(myEvent);
                        }
                        break;
                }
            }
	
        }

        /// <summary>
        /// sets the widths of the event boxes based their events' time spans
        /// </summary>
        public void setWidths()
        {
            for (int i = 0; i < _events.Count; i++)
            {
                _events.ElementAt(i).computeWidth(_distanceBetweenTicks);
            }
        }

        /// <summary>
        /// returns a list of events
        /// </summary>
        /// <returns></returns>
        public List<Event> getEvents()
        {
            return _events;
        }

        /// <summary>
        /// sorts events (based on their start year)
        /// </summary>
        /// <param name="events"></param>
        public static void sortEvents(List<Event> events)
        {
            events.Sort();        
        }

        /// <summary>
        /// assigns each event (in sorted order) one of 4 alternating colors
        /// </summary>
        /// <param name="events"></param>
        public static void colorEvents(List<Event> events)
        {
            if (events.Count > 0)
            {
                for (int i = 0; i < events.Count; i++)
                {
                    events.ElementAt(i).setColor(Color.FromRgb(0xe9, 0x5a, 0x4f));
                    /*
                    if (i % 4 == 0)
                    {
                        events.ElementAt(i).setColor(Color.FromRgb(152, 245, 255));
                    }
                    else if (i % 4 == 1)
                    {
                        events.ElementAt(i).setColor(Color.FromRgb(238,238,0));
                    }
                    else if (i % 4 == 2)
                    {
                        events.ElementAt(i).setColor(Color.FromRgb(124, 252, 0));
                    }
                    else
                    {
                        events.ElementAt(i).setColor(Color.FromRgb(250, 128, 114));
                    }*/
                }
            }

        }

    }
}

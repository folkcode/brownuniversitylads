using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LADSArtworkMode
{
    public class NameInfo
    {
        private string _dateReceived = "";
        public string DateReceived { set { _dateReceived = this.sanitizeString(value); } get { return _dateReceived; } }
        private string _makerFirst = "";
        public string MakerFirst { set { _makerFirst = this.sanitizeString(value); } get { return _makerFirst; } }
        private string _makerLast = "";
        public string MakerLast { set { _makerLast = this.sanitizeString(value); } get { return _makerLast; } }
        private string _panelFirst = "";
        public string PanelFirst { set { _panelFirst = this.sanitizeString(value); } get { return _panelFirst; } }
        private string _panelLast = "";
        public string PanelLast { set { _panelLast = this.sanitizeString(value); } get { return _panelLast; } }
        public string _panelName = "";
        public string PanelName { get { return _panelName; } }
        private string _city = "";
        public string City { set { _city = this.sanitizeString(value); } get { return _city; } }
        private string _otherCities = "";
        public string OtherCities { set { _otherCities = this.sanitizeString(value); } get { return _otherCities; } }
        private string _state = "";
        public string State { set { _state = this.sanitizeString(value); } get { return _state; } }
        private string _country = "";
        public string Country { set { _country = this.sanitizeCountryString(value); } get { return _country; } }
        private string _cityFull = "";
        public string CityFull { get { return _cityFull; } }
        private string _dob = "";
        public string Dob { set { _dob = this.sanitizeString(value); } get { return _dob; } }
        private string _dod = "";
        public string Dod { set { _dod = this.sanitizeString(value); } get { return _dod; } }
        private string _panelNumber;
        public string PanelNumber { set { _panelNumber = this.sanitizeString(value); } get { return _panelNumber; } }
        /*private int _panelNumber;  it's more than just a number; so annoying!!
        public int PanelNumber { set { _panelNumber = value; } get { return _panelNumber; } }*/
        private int _block;
        public int Block { set { _block = value; } get { return _block; } }

        public void ParsePanelName()
        {
            _panelName = (_panelFirst.Equals("")) ? _panelLast : _panelFirst + " " + _panelLast;
            //_panelName = _panelFirst + " " + _panelLast;
            _panelName.Trim();
            _panelName = (_panelName.Equals("")) ? "UNKNOWN" : _panelName;
            _panelName.Trim();
        }

        public void ParseCityFull()
        {
            _cityFull = _city.Equals("") ? _state : _city + ", " + _state;
            _cityFull = _cityFull.Equals("") ? _cityFull : _cityFull + " " + _country;
            _cityFull.Trim();
        }

        private string sanitizeCountryString(string country)
        {
            if (country.Equals("NULL"))
            {
                country = "USA";
            }
            return this.sanitizeString(country);
        }

        private string sanitizeString(string s)
        {
            if (s.Equals("1") || s.Equals("NULL") || s.Equals(" "))
            {
                    s = "";
            }
            return s;
        }
    }
}

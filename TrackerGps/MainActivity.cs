using Android.App;
using Android.Widget;
using Android.OS;
using Core;
using System;
using Android.Locations;
using System.Collections.Generic;
using Android.Util;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Plugin.Geolocator;
using System.Threading;
using Plugin.DeviceInfo;
using Android;
using Android.Content.PM;

namespace TrackerGps
{
    [Activity(Label = "TrackerGps", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity, ILocationListener
    {
        public static DateTime DateSaveGeo { get; set; }
        public static string IdDevice { get; set; }
        static readonly string TAG = "X:" + typeof(MainActivity).Name;
        TextView _addressText;
        public static TextView _erreur;
        Location _currentLocation;
        LocationManager _locationManager;

        string _locationProvider;
        TextView _locationText;

        


        public async void OnLocationChanged(Location location)
        {
            _currentLocation = location;
            if (_currentLocation == null)
            {
                _locationText.Text = "Unable to determine your location. Try again in a short while.";
            }
            else
            {
                //_locationText.Text = string.Format("{0:f6},{1:f6}", _currentLocation.Latitude, _currentLocation.Longitude);
                Address address = await ReverseGeocodeCurrentLocation();
                DisplayAddress(address);

            }
        }
        public void OnProviderDisabled(string provider) { }

        public void OnProviderEnabled(string provider) { }

        public void OnStatusChanged(string provider, Availability status, Bundle extras)
        {
            Log.Debug(TAG, "{0}, {1}", provider, status);
        }
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.Main);

            _addressText = FindViewById<TextView>(Resource.Id.address_text);
            _locationText = FindViewById<TextView>(Resource.Id.location_text);
            _erreur = FindViewById<TextView>(Resource.Id.erreur);
            FindViewById<TextView>(Resource.Id.get_address_button).Click += AddressButton_OnClick;
            FindViewById<TextView>(Resource.Id.clear).Click += clear_OnClick;

            InitializeLocationManager();
        }
        
        void InitializeLocationManager()
        {
            
            try
            {
                Android.Telephony.TelephonyManager mTelephonyMgr;
                //Telephone Number  
                mTelephonyMgr = (Android.Telephony.TelephonyManager)GetSystemService(TelephonyService);
                

                //IMEI number  
                IdDevice = mTelephonyMgr.DeviceId;
            }
            catch (Exception err)
            {
                _erreur.Text = err.Message;
                IdDevice= CrossDevice.Network.IpAddress;
            }
            
            DateSaveGeo = DateTime.Now.ToUniversalTime();
            _locationManager = (LocationManager)GetSystemService(LocationService);
            Criteria criteriaForLocationService = new Criteria
            {
                Accuracy = Accuracy.Fine
            };
            IList<string> acceptableLocationProviders = _locationManager.GetProviders(criteriaForLocationService, true);

            if (acceptableLocationProviders.Any())
            {
                _locationProvider = acceptableLocationProviders.First();
            }
            else
            {
                _locationProvider = string.Empty;
            }
            Log.Debug(TAG, "Using " + _locationProvider + ".");
        }

        private void Current_PositionChanged(object sender, Plugin.Geolocator.Abstractions.PositionEventArgs e)
        {
            var test = e.Position;
            _locationText.Text = string.Format("{0:f6},{1:f6}", test.Latitude.ToString(), test.Longitude.ToString());
            wsEcriture(test.Latitude.ToString(), test.Longitude.ToString());
            //SavePoint.SaveLatLong(IdDevice, test.Latitude.ToString(), test.Longitude.ToString());
        }
        async void wsEcriture(string _longitude,string _latitude)
        {
            SavePoint.SaveLatLong(IdDevice, _latitude, _longitude);
        }

        protected override void OnResume()
        {
            base.OnResume();
            CrossGeolocator.Current.StartListeningAsync(TimeSpan.FromSeconds(5), 1, true, new Plugin.Geolocator.Abstractions.ListenerSettings
            {
                ActivityType = Plugin.Geolocator.Abstractions.ActivityType.AutomotiveNavigation,
                AllowBackgroundUpdates = false,
                DeferLocationUpdates = false,
                DeferralDistanceMeters = 1,
                DeferralTime = TimeSpan.FromSeconds(1),
                ListenForSignificantChanges = false,
                PauseLocationUpdatesAutomatically = false
            });

            CrossGeolocator.Current.PositionChanged += Current_PositionChanged;
            Log.Debug(TAG, "Listening for location updates using " + _locationProvider + ".");
        }

        protected override void OnPause()
        {
            base.OnPause();
            _locationManager.RemoveUpdates(this);
            Log.Debug(TAG, "No longer listening for location updates.");
        }

        void clear_OnClick(object sender, EventArgs eventArgs)
        {
            _erreur.Text = "Erreur (when available)";
        }

        async void AddressButton_OnClick(object sender, EventArgs eventArgs)
        {
            if (_currentLocation == null)
            {
                _addressText.Text = "Can't determine the current address. Try again in a few minutes.";
                return;
            }

            Address address = await ReverseGeocodeCurrentLocation();
            DisplayAddress(address);
        }
        async Task<Address> ReverseGeocodeCurrentLocation()
        {
            Geocoder geocoder = new Geocoder(this);
            IList<Address> addressList =
                await geocoder.GetFromLocationAsync(_currentLocation.Latitude, _currentLocation.Longitude, 10);

            Address address = addressList.FirstOrDefault();
            return address;
        }

        void DisplayAddress(Address address)
        {
            if (address != null)
            {
                StringBuilder deviceAddress = new StringBuilder();
                for (int i = 0; i < address.MaxAddressLineIndex; i++)
                {
                    deviceAddress.AppendLine(address.GetAddressLine(i));
                }
                // Remove the last comma from the end of the address.
                _addressText.Text = deviceAddress.ToString();
            }
            else
            {
                _addressText.Text = "Unable to determine the address. Try again in a few minutes.";
            }
        }

    }

   
}


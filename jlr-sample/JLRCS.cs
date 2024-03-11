using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Text.Json;
using System.Threading.Tasks;
using System.Reflection.Metadata;

namespace jlr_sample
{

    // Porting from JLRPY Python Project on GitHub from user 'ardevd'
    // https://github.com/ardevd/jlrpy
    // All honours for the JLR coding logic goes to that project
    //
    // Porting has been done fully manually and source code is based on C# 8 standard
    // Class extensions to Json and String classes have been added to better align with python coding
    // 
    // The library structure has been kept as equal as possible to the original python code
    // Method names have been converted to C# CamelCase standard to satisfy LINT
    // Property names have been untouched

    public class JLRCS
    {
        internal static JsonObject BaseURLs = new JsonObject()
        {
            // Rest Of World Base URLs
            { "IFAS" , "https://ifas.prod-row.jlrmotor.com/ifas/jlr" },
            { "IFOP" , "https://ifop.prod-row.jlrmotor.com/ifop/jlr" },
            { "IF9" , "https://if9.prod-row.jlrmotor.com/if9/jlr" }
        };

        internal static JsonObject ChinaBaseURLs = new JsonObject()
        {
            // China Base URLs
            { "IFAS" , "https://ifas.prod-chn.jlrmotor.com/ifas/jlr" },
            { "IFOP" , "https://ifop.prod-chn.jlrmotor.com/ifop/jlr" },
            { "IF9" , "https://ifoa.prod-chn.jlrmotor.com/if9/jlr"}
        };

        private const int TIMEOUT = 30; // Define TIMEOUT value


        public class Connection
        {
            // Connection to the JLR Remote Car API

            public string? email { get; set; }
            private long expiration { get; set; }
            public string? access_token { get; set; }
            public string? auth_token { get; set; }
            internal JsonObject head { get; set; }
            private string? refresh_token { get; set; }
            public string? user_id { get; set; }
            public List<Vehicle>? vehicles { get; set; }
            public JsonObject? user_data { get; set; }

            internal JsonObject base_urls { get; set; }

            public string? device_id { get; set; }
            private JsonObject? oauth { get; set; }

            private Logger logger = Logger.Instance;


            public Connection(string? email = "",
                              string? password = "",
                              string? device_id = "",
                              string? refresh_token = "",
                              bool use_china_servers = false)
            {
                /* Init the connection object
 
                The email address and password associated with your Jaguar InControl account is required.
                A device Id can optionally be specified.If not one will be generated at runtime.
                A refresh token can be supplied for authentication instead of a password
                */
                this.email = email;
                this.expiration = 0;
                this.access_token = null;
                this.auth_token = null;
                this.head = new JsonObject();
                this.refresh_token = refresh_token;
                this.user_id = null;
                this.vehicles = new List<Vehicle>();

                if (use_china_servers)
                {
                    this.base_urls = ChinaBaseURLs;
                }
                else
                {
                    this.base_urls = BaseURLs;
                }


                if (!string.IsNullOrEmpty(device_id))
                {
                    this.device_id = device_id;
                }
                else
                {
                    this.device_id = Guid.NewGuid().ToString();
                }

                if (!string.IsNullOrEmpty(refresh_token))
                {
                    this.oauth = new JsonObject()
                    {
                        { "grant_type", "refresh_token" },
                        { "refresh_token", refresh_token }
                    };
                }
                else
                {
                    this.oauth = new JsonObject()
                    {
                        { "grant_type", "password" },
                        { "username", email },
                        { "password", password }
                    };
                }

                this.Connect();

                if (this.access_token != null)
                {
                    try
                    {
                        JsonArray? get_vehicles = this.GetVehicles(this.head)!["vehicles"] as JsonArray;
                        foreach (JsonNode? vehicle in get_vehicles!)
                        {
                            this.vehicles.Add(new Vehicle(vehicle! as JsonObject, this));

                        }

                    }
                    catch (Exception ex)
                    {
                        logger.Error("No vehicles associated with this account");
                    }
                }
            }

            private void _ValidateToken()
            {
                // Is token still valid
                long now = (long)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;
                if (now > expiration)
                {
                    // Auth expired, reconnect
                    this.Connect();
                }
            }

            internal JsonObject? __Get(string command, string url, JsonObject headers)
            {
                // GET data from API
                this._ValidateToken();
                if (headers.ContainsKey("Authorization"))
                {
                    headers["Authorization"] = this.head["Authorization"]!.ToString();
                }
                return this.__Request($"{url}/{command}", headers : headers, method : "GET");
            }

            internal JsonObject? __Post(string command, string url, JsonObject headers, JsonObject? data = null)
            {
                // POST data to API
                this._ValidateToken();
                if (headers.ContainsKey("Authorization"))
                {
                    headers["Authorization"] = this.head["Authorization"]!.ToString();
                }
                return this.__Request($"{url}/{command}", headers: headers, data : data, method: "POST");
            }

            internal JsonObject? __Delete(string command, string url, JsonObject headers)
            {
                // DELETE data from API
                this._ValidateToken();
                if (headers.ContainsKey("Authorization"))
                {
                    headers["Authorization"] = this.head["Authorization"]!.ToString();
                }
                if (headers.ContainsKey("Accept"))
                {
                    headers.Remove("Accept");
                }
                return this.__Request($"{url}/{command}", headers: headers, method: "DELETE");
            }

            public void Connect()
            {
                // Connect to JLR API
                logger.Info("Connecting...");
                JsonObject? auth = this._Authenticate(data: this.oauth);
                if (auth != null)
                {
                    this._RegisterAuth(auth);
                    this._SetHeader(auth["access_token"]!.ToString());
                    logger.Info("[+] authenticated");
                    this._RegisterDeviceAndLogIn();
                }
                else
                {
                    logger.Info("[-] not authenticated");
                }
            }


            private void _RegisterDeviceAndLogIn()
            {
                this._RegisterDevice(head);
                logger.Info("1/2 device id registered");
                this._LoginUser(head);
                logger.Info("2/2 user logged in, user id retrieved");
            }


            private JsonObject? __Request(string url, JsonObject? headers = null, JsonObject? data = null, string method = "GET")
            {
                logger.Debug(method + "-" + url);
                Task<JsonObject?> asyncTask = Task.Run(() => this.___RequestAsync(url, headers, data, method));
                asyncTask.Wait();
                return asyncTask.Result;
            }

            private async Task<JsonObject?> ___RequestAsync(string url, JsonObject? headers = null, JsonObject? data = null, string method = "GET")
            {
                using (HttpClient client = new HttpClient())
                {
                    HttpRequestMessage request = new HttpRequestMessage(new HttpMethod(method), url);
                    client.Timeout = TimeSpan.FromSeconds(TIMEOUT);

                    string content_type = "application/json";

                    if (headers != null)
                    {
                        foreach (KeyValuePair<string, JsonNode?> header in headers)
                        {
                            try
                            {
                                // don't set Content-Type here in the Headers, but use it when setting Content furter down
                                if (header.Key.CompareTo("Content-Type") == 0)
                                {
                                    content_type = header.Value!.ToString();
                                }
                                else
                                {
                                    logger.Debug(header.Key + " = " + header.Value!.ToString());
                                    request.Headers.Add(header.Key, header.Value!.ToString());
                                }
                            }
                            catch (Exception ex)
                            {
                                logger.Error(ex.Message);
                            }
                        }
                    }

                    if (data != null)
                    {
                        string jsonData = JsonSerializer.Serialize(data);
                        logger.Debug("Data = " + jsonData);
                        request.Content = new StringContent(jsonData, Encoding.UTF8, content_type);
                    }

                    try
                    {
                        HttpResponseMessage response = await client.SendAsync(request);

                        if (response.IsSuccessStatusCode)
                        {
                            string responseContent = await response.Content.ReadAsStringAsync();
                            logger.Debug("Result = " + responseContent);
                            if (!string.IsNullOrEmpty(responseContent))
                            {
                                return JsonNode.Parse(responseContent) as JsonObject;
                            }
                        }
                        else
                        {
                            logger.Fatal("No Success");
                        }
                    }
                    catch (HttpRequestException hrex)
                    {
                        // Handle request exception
                        logger.Error(hrex.Message);
                    }
                }
                return null;
            }


            private void _RegisterAuth(JsonObject auth)
            {
                this.access_token = auth["access_token"]!.ToString();
                int now = (int)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;
                this.expiration = now + int.Parse(auth["expires_in"]!.ToString());
                this.auth_token = auth["authorization_token"]!.ToString();
                this.refresh_token = auth["refresh_token"]!.ToString();
            }

            private void _SetHeader(string access_token)
            {
                // Set HTTP header fields
                this.head = new JsonObject()
                {
                    { "Authorization", $"Bearer {access_token}" },
                    { "X-Device-Id", this.device_id },
                    { "x-telematicsprogramtype", "jlrpy" },
                    { "x-App-Id", "ICR_JAGUAR" },
                    { "x-App-Secret", "018dd168-6271-707f-9fd4-aed2bf76905e" },
                    { "Content-Type", "application/json" }
                };
            }

            private JsonObject? _Authenticate(JsonObject? data = null)
            {
                // Raw urlopen command to the auth url
                string url = $"{this.base_urls["IFAS"]}/tokens";
                JsonObject auth_headers = new JsonObject()
                {
                    { "Authorization", "Basic YXM6YXNwYXNz" },
                    { "Content-Type", "application/json" },
                    { "X-Device-Id", this.device_id }
                };
                return this.__Request(url, auth_headers, data, "POST");
            }

            private JsonObject? _RegisterDevice(JsonObject? headers = null)
            {
                // Register the device Id
                string url = $"{this.base_urls["IFOP"]}/users/{this.email}/clients";
                JsonObject data = new JsonObject()
                {
                    { "access_token", this.access_token },
                    { "authorization_token", this.auth_token },
                    { "expires_in", "86400" },
                    { "deviceID", this.device_id }
                };
                return this.__Request(url, headers!, data, "POST");
            }

            private JsonObject? _LoginUser(JsonObject? headers = null)
            {
                // Login the user
                string url = $"{this.base_urls["IF9"]}/users?loginName={this.email}";
                JsonObject? user_login_header = headers!.Copy();
                user_login_header!["Accept"] = "application/vnd.wirelesscar.ngtp.if9.User-v3+json";

                this.user_data = this.__Request(url, user_login_header);
                this.user_id = user_data!["userId"]!.ToString();
                return user_data;
            }

            public void RefreshTokens(string access_token)
            {
                // Refresh tokens
                this.oauth = new JsonObject()
                {
                    { "grant_type", "refresh_token" },
                    { "refresh_token", this.refresh_token}
                };
                JsonObject? auth = this._Authenticate(this.oauth);
                this._RegisterAuth(auth!);
                this._SetHeader(auth!["access_token"]!.ToString());
                logger.Info("[+] Tokens refreshed");
                this._RegisterDeviceAndLogIn();
            }

            public JsonObject? GetVehicles(JsonObject? headers)
            {
                // Get vehicles for user
                string url = $"{this.base_urls["IF9"]}/users/{this.user_id}/vehicles?primaryOnly=true";
                return this.__Request(url, headers);
            }

            public JsonObject? GetUserInfo()
            {
                // Get user information
                JsonObject? headers = this.head!.Copy();
                headers!["Accept"] = "application/vnd.wirelesscar.ngtp.if9.User-v3+json";
                headers!["Content-Type"] = "application/json";
                return this.__Get(this.user_id!, this.base_urls["IF9"] + "/users", this.head);
            }

            public JsonObject? UpdateUserInfo(JsonObject user_info_data)
            {
                // Update user information
                JsonObject? headers = this.head!.Copy();
                headers!["Content-Type"] = "application/vnd.wirelesscar.ngtp.if9.User-v3+json";
                return this.__Post(this.user_id!, this.base_urls["IF9"] + "/users", headers, user_info_data);
            }

            public JsonObject? ReverseGeoCode(string lat, string lon)
            {
                // Get geocode information
                JsonObject? headers = this.head!.Copy();
                headers!["Accept"] = "application/json";
                return this.__Get("en", $"{this.base_urls["IF9"]}/geocode/reverse/{lat}/{lon}", headers);
            }
        }


        public class Vehicle
        {
            /*
            Vehicle class.

            You can request data or send commands to vehicle. Consult the JLR API documentation for details
            */

            private JsonObject? dict { get; set; }
            private Connection? connection { get; set; }
            public string? vin { get; set; }

            public Vehicle(JsonObject? vehicle)
            {
                this.dict = vehicle;
            }

            public Vehicle(JsonObject? vehicle, Connection connection) : this(vehicle)
            {
                this.connection = connection;
                this.vin = vehicle!["vin"]!.ToString();
            }

            public JsonObject? GetContactInfo(string mcc)
            {
                // Get contact info for the specified mobile country code
                JsonObject? headers = this.connection!.head!.Copy();
                return this.__Get($"contactinfo/{mcc}", headers!);
            }

            public JsonObject? GetAttributes()
            {
                // Get vehicle attributes
                JsonObject? headers = this.connection!.head!.Copy();
                headers!["Accept"] = "application/vnd.ngtp.org.VehicleAttributes-v8+json";
                return this.__Get("attributes", headers);
            }

            public JsonObject? GetStatus(string? key = null)
            {
                // Get vehicle status
                JsonObject? headers = this.connection!.head!.Copy();
                headers!["Accept"] = "application/vnd.ngtp.org.if9.healthstatus-v4+json";
                JsonObject? result = this.__Get("status?includeInactive=true", headers);

                if (!string.IsNullOrEmpty(key))
                {
                    JsonArray? core_status = result!["vehicleStatus"]!["coreStatus"] as JsonArray;
                    JsonArray? ev_status = result!["vehicleStatus"]!["evStatus"] as JsonArray;
                    core_status!.AddRange(ev_status!);

                    return core_status![key] as JsonObject;
                }
                return result;
            }

            public JsonObject? GetHealthStatus()
            {
                // Get vehicle health status
                JsonObject? headers = this.connection!.head!.Copy();
                headers!["Accept"] = "application/vnd.wirelesscar.ngtp.if9.ServiceStatus-v4+json";
                headers!["Content-Type"] = "application/vnd.wirelesscar.ngtp.if9.StartServiceConfiguration-v3+json";

                JsonObject? vhs_data = this._AuthenticateVHS();

                return this.__Post("healthstatus", headers, vhs_data);
            }

            public JsonObject? GetDepartureTimers()
            {
                // Get vehicle departure timers
                JsonObject? headers = this.connection!.head!.Copy();
                headers!["Accept"] = "application/vnd.wirelesscar.ngtp.if9.DepartureTimerSettings-v1+json";
                return this.__Get("departuretimers", headers);
            }

            public JsonObject? GetWakeupTime()
            {
                // Get configured wakeup time for vehicle
                JsonObject? headers = this.connection!.head!.Copy();
                headers!["Accept"] = "application/vnd.wirelesscar.ngtp.if9.VehicleWakeupTime-v2+json";
                return this.__Get("wakeuptime", headers);
            }

            public JsonObject? GetSubscriptionPackages()
            {
                // Get vehicle status
                return this.__Get("subscriptionpackages", this.connection!.head);
            }

            public JsonObject? GetTrips(int count = 1000)
            {
                // Get the last 1000 trips associated with vehicle
                JsonObject? headers = this.connection!.head!.Copy();
                headers!["Accept"] = "application/vnd.ngtp.org.triplist-v2+json";
                return this.__Get($"trips?count={count}", headers);
            }

            public JsonObject? GetGuardianModeAlarms()
            {
                // Get Guardian Mode Alarms
                JsonObject? headers = this.connection!.head!.Copy();
                headers!["Accept"] = "application/vnd.wirelesscar.ngtp.if9.GuardianStatus-v1+json";
                headers!["Accept-Encoding"] = "gzip,deflate";
                return this.__Get("gm/alarms", headers);
            }

            public JsonObject? GetGuardianModeAlerts()
            {
                // Get Guardian Mode Alerts
                JsonObject? headers = this.connection!.head!.Copy();
                headers!["Accept"] = "application/wirelesscar.GuardianAlert-v1+json";
                headers!["Accept-Encoding"] = "gzip,deflate";
                return this.__Get("gm/alerts", headers);
            }

            public JsonObject? GetGuardianModeStatus()
            {
                // Get Guardian Mode Status
                JsonObject? headers = this.connection!.head!.Copy();
                headers!["Accept"] = "application/vnd.wirelesscar.ngtp.if9.GuardianStatus-v1+json";
                return this.__Get("gm/status", headers);
            }

            public JsonObject? GetGuardianModeSettingsUser()
            {
                // Get Guardian Mode User Settings
                JsonObject? headers = this.connection!.head!.Copy();
                headers!["Accept"] = "application/vnd.wirelesscar.ngtp.if9.GuardianUserSettings-v1+json";
                return this.__Get("gm/settings/user", headers);
            }

            public JsonObject? GetGuardianModeSettingsSystem()
            {
                // Get Guardian Mode System Settings
                JsonObject? headers = this.connection!.head!.Copy();
                headers!["Accept"] = "application/vnd.wirelesscar.ngtp.if9.GuardianSystemSettings-v1+json";
                return this.__Get("gm/settings/system", headers);
            }

            public JsonObject? GetTrip(string trip_id, int section = 1)
            {
                // Get info on a specific trip
                return this.__Get($"trips/{trip_id}/route?pageSize=1000&page={section}", this.connection!.head);
            }

            public JsonObject? GetPosition()
            {
                // Get current vehicle position
                return this.__Get("position", this.connection!.head);
            }

            public JsonObject? GetServiceStatus(string service_id)
            {
                // Get service status
                JsonObject? headers = this.connection!.head!.Copy();
                headers!["Accept"] = "application/vnd.wirelesscar.ngtp.if9.ServiceStatus-v4+json";
                return this.__Get($"services/{service_id}", headers);
            }

            public JsonObject? GetServices()
            {
                // Get active services
                JsonObject? headers = this.connection!.head!.Copy();
                return this.__Get("srvices", headers!);
            }

            public JsonObject? GetRCCTargetValue()
            {
                // Get Remote Climate Target Value
                JsonObject? headers = this.connection!.head!.Copy();
                return this.__Get("settings/ClimateControlRccTargetTemp", headers!);
            }

            public JsonObject? SetAttributes(string nickname, string registration_number)
            {
                // Set vehicle nickname and registration number
                JsonObject attributes_data = new JsonObject()
                {
                    { "nickname", nickname },
                    { "registrationNumber", registration_number }
                };

                return this.__Post("attributes", this.connection!.head, attributes_data);
            }

            public JsonObject? Lock(string pin)
            {
                // Lock vehicle. Requires personal PIN for authentication
                JsonObject? headers = this.connection!.head!.Copy();
                headers!["Content-Type"] = "application/vnd.wirelesscar.ngtp.if9.StartServiceConfiguration-v3+json";
                JsonObject? rdl_data = this._AuthenticateRDL(pin);
                return this.__Post("lock", headers, rdl_data);
            }

            public JsonObject? Unlock(string pin)
            {
                // Lock vehicle. Requires personal PIN for authentication
                JsonObject? headers = this.connection!.head!.Copy();
                headers!["Content-Type"] = "application/vnd.wirelesscar.ngtp.if9.StartServiceConfiguration-v3+json";
                JsonObject? rdu_data = this._AuthenticateRDU(pin);
                return this.__Post("unlock", headers, rdu_data);
            }

            public JsonObject? ResetAlarm(string pin)
            {
                // Reset vehicle alarm
                JsonObject? headers = this.connection!.head!.Copy();
                headers!["Content-Type"] = "application/vnd.wirelesscar.ngtp.if9.StartServiceConfiguration-v3+json";
                headers!["Accept"] = "application/vnd.wirelesscar.ngtp.if9.ServiceStatus-v4+json";
                JsonObject? aloff_data = this._AuthenticateALOFF(pin);
                return this.__Post("unlock", headers, aloff_data);
            }

            public JsonObject? HonkBlink()
            {
                // Sound the horn and blink lights
                JsonObject? headers = this.connection!.head!.Copy();
                headers!["Accept"] = "application/vnd.wirelesscar.ngtp.if9.ServiceStatus-v4+json";
                headers!["Content-Type"] = "application/vnd.wirelesscar.ngtp.if9.StartServiceConfiguration-v3+json";
                JsonObject? hblf_data = this._AuthenticateHBLF();
                return this.__Post("honkBlink", headers, hblf_data);
            }

            public JsonObject? RemoteEngineStart(string pin, int target_value)
            {
                // Start Remote Engine preconditioning
                JsonObject? headers = this.connection!.head!.Copy();
                headers!["Content-Type"] = "application/vnd.wirelesscar.ngtp.if9.StartServiceConfiguration-v2+json";
                this._SetRCCTargetValue(pin, target_value);
                JsonObject? reon_data = this._AuthenticateREON(pin);
                return this.__Post("engineOn", headers, reon_data);
            }

            public JsonObject? RemoteEngineStop(string pin, int target_value)
            {
                // Stop Remote Engine preconditioning
                JsonObject? headers = this.connection!.head!.Copy();
                headers!["Content-Type"] = "application/vnd.wirelesscar.ngtp.if9.StartServiceConfiguration-v2+json";

                JsonObject? reoff_data = this._AuthenticateREOFF(pin);
                return this.__Post("engineOff", headers, reoff_data);
            }

            private JsonObject? _SetRCCTargetValue(string pin, int target_value)
            {
                // Set Remote Climate Target Value (value between 31-57, 31 is LO 57 is HOT)
                JsonObject? headers = this.connection!.head!.Copy();
                this.EnableProvisioningMode(pin);
                JsonObject service_parameters = new JsonObject()
                {
                    { "key", "ClimateControlRccTargetTemp" },
                    { "value", target_value.ToString() },
                    { "applied", 1 }
                };

                return this.__Post("settings", headers!, service_parameters);
            }

            public JsonObject? GetWAUAStatus()
            {
                // Get WAUA status
                JsonObject? headers = this.connection!.head!.Copy();
                headers!["Accept"] = "application/wirelesscar.WauaStatus-v1+json";
                return this.__Get("waua/status", headers!);
            }

            public JsonObject? PreconditioningStart(int target_temp)
            {
                // Start pre-conditioning for specified temperature (celsius)
                JsonObject? service_parameters = new JsonObject().Create(
                    new[]
                        { new { key = "PRECONDITIONING", value = "START" },
                          new { key = "TARGET_TEMPERATURE_CELSIUS", value = target_temp.ToString() } }
                );
                return this._PreconditioningControl(service_parameters);
            }

            public JsonObject? PreconditioningStop()
            {
                // Stop climate preconditioning
                JsonObject? service_parameters = new JsonObject().Create(
                    new[]
                        { new { key = "PRECONDITIONING",
                                value = "STOP" } }
                );
                return this._PreconditioningControl(service_parameters);
            }

            public JsonObject? ClimatePrioritize(string priority)
            {
                // Optimize climate controls for comfort or range
                JsonObject? service_parameters = new JsonObject().Create(
                    new[]
                        { new { key = "PRIORITY_SETTING",
                                value = priority } }
                );
                return this._PreconditioningControl(service_parameters);
            }

            private JsonObject? _PreconditioningControl(JsonObject? service_parameters)
            {
                // Control the climate preconditioning
                JsonObject? headers = this.connection!.head!.Copy();
                headers!["Accept"] = "application/vnd.wirelesscar.ngtp.if9.ServiceStatus-v5+json";
                headers!["Content-Type"] = "application/vnd.wirelesscar.ngtp.if9.PhevService-v1+json";

                JsonObject? ecc_data = this._AuthenticateECC();
                ecc_data!["serviceParameters"] = service_parameters;
                return this.__Post("preconditioning", headers, ecc_data);
            }

            public JsonObject? ChargingStop()
            {
                // Stop charging
                JsonObject? service_parameters = new JsonObject().Create(
                    new[]
                        { new { key = "CHARGE_NOW_SETTING",
                                value = "FORCE_OFF" } }
                );
                return this._ChargingProfileControl("serviceParameters", service_parameters!);
            }

            public JsonObject? ChargingStart()
            {
                // Start charging
                JsonObject? service_parameters = new JsonObject().Create(
                    new[]
                        { new { key = "CHARGE_NOW_SETTING",
                                value = "FORCE_ON" } }
                );
                return this._ChargingProfileControl("serviceParameters", service_parameters!);
            }


            public JsonObject? SetMaxSOC(int max_charge_level)
            {
                // Set max state of charge in percentage
                JsonObject? service_parameters = new JsonObject().Create(
                    new[]
                        { new { key = "SET_PERMANENT_MAX_SOC",
                                value = max_charge_level } }
                );
                return this._ChargingProfileControl("serviceParameters", service_parameters!);
            }

            public JsonObject? SetOneOffMaxSOC(int max_charge_level)
            {
                // Set one off max state of charge in percentage
                JsonObject? service_parameters = new JsonObject().Create(
                    new[]
                        { new { key = "SET_ONE_OFF_MAX_SOC",
                                value = max_charge_level } }
                );
                return this._ChargingProfileControl("serviceParameters", service_parameters!);
            }

            public JsonObject? AddDepartureTimer(int index, int departure_year, int departure_month, int departure_day, int departure_hour, int departure_minute)
            {
                // Add a single departure timer with the specified index
                JsonObject? departure_timer_setting = new JsonObject().Create(
                         new
                         {
                             timers = new[]
                                { new { departureTime = new { hour = departure_hour, minute = departure_minute },
                                        timerIndex = index,
                                        timerTarget = new { singleDay = new {   day = departure_day,
                                                                                month = departure_month,
                                                                                year = departure_year } },
                                        timerType = new { key = "BOTHCHARGEANDPRECONDITION", value = true } } }
                         }
                );

                return this._ChargingProfileControl("departureTimerSetting", departure_timer_setting!);
            }

            public JsonObject? AddRepeatedDepartureTimer(int index, string schedule, int departure_hour, int departure_minute)
            {
                // Add repeated departure timer.
                JsonObject? departure_timer_setting = new JsonObject().Create(
                         new
                         {
                             timers = new[]
                                { new { departureTime = new { hour = departure_hour, minute = departure_minute },
                                        timerIndex = index,
                                        timerTarget = new { repeatSchedule = schedule },
                                        timerType = new { key = "BOTHCHARGEANDPRECONDITION", value = true } } }
                         }
                );

                return this._ChargingProfileControl("departureTimerSetting", departure_timer_setting!);
            }

            public JsonObject? DeleteDepartureTimer(int index)
            {
                // Delete a single departure timer associated with the specified index
                JsonObject? departure_timer_setting = new JsonObject().Create(
                       new
                       {
                           timers = new[]
                                { new { timerIndex = index } }
                       }
                );
                return this._ChargingProfileControl("departureTimerSetting", departure_timer_setting!);
            }

            public JsonObject? AddChargingPeriod(int index, int schedule, int hour_from, int minute_from, int hour_to, int minute_to)
            {
                // Add charging period
                JsonObject? tariff_settings = new JsonObject().Create(
                        new
                        {
                            tariffs = new[]
                                { new { tariffIndex = index,
                                        tariffDefinition = new { enabled = true,
                                                                 repeatSchedule = schedule,
                                                                 tariffZone = new []
                                                                 {  new {   zoneName = "TARIFF_ZONE_A",
                                                                            bandType = "PEAK",
                                                                            endTime = new {
                                                                               hour = hour_from,
                                                                               minute = minute_from } },
                                                                    new {   zoneName = "TARIFF_ZONE_B",
                                                                            bandType =  "OFFPEAK",
                                                                            endTime = new {
                                                                               hour = hour_to,
                                                                               minute = minute_to } },
                                                                    new {   zoneName = "TARIFF_ZONE_C",
                                                                            bandType = "PEAK",
                                                                            endTime = new {
                                                                               hour = 0,
                                                                               minute=  0 } } } } } }
                        }
                );
                return this._ChargingProfileControl("tariffSettings", tariff_settings!);
            }

            private JsonObject? _ChargingProfileControl(string service_parameter_key, JsonObject service_parameters)
            {
                // Charging profile API
                JsonObject? headers = this.connection!.head!.Copy();
                headers!["Accept"] = "application/vnd.wirelesscar.ngtp.if9.ServiceStatus-v5+json";
                headers!["Content-Type"] = "application/vnd.wirelesscar.ngtp.if9.PhevService-v1+json";

                JsonObject? cp_data = this._AuthenticateCP();
                cp_data![service_parameter_key] = service_parameters;

                return this.__Post("chargeProfile", headers, cp_data);
            }

            public JsonObject? SetWakeupTime(long wakeup_time)
            {
                // Set the wakeup time for the specified time (epoch milliseconds)
                JsonObject? swu_data = this._AuthenticateSWU();
                swu_data!["serviceCommand"] = "START";
                swu_data!["startTime"] = wakeup_time;
                return this._SWU(swu_data);
            }

            public JsonObject? SetWakeupTime(DateTime wakeup_time)
            {
                // Set the wakeup time for the specified time 
                long exp = (long)(wakeup_time.ToUniversalTime() - new DateTime(1970, 1, 1)).TotalMilliseconds;
                return this.SetWakeupTime(exp);
            }

            public JsonObject? DeleteWakeupTime()
            {
                // Stop the wakeup time
                JsonObject? swu_data = this._AuthenticateSWU();
                swu_data!["serviceCommand"] = "END";
                return this._SWU(swu_data);
            }

            private JsonObject? _SWU(JsonObject swu_data)
            {
                // Set the wakeup time for the specified time (epoch milliseconds)
                JsonObject? headers = this.connection!.head!.Copy();
                headers!["Accept"] = "application/vnd.wirelesscar.ngtp.if9.ServiceStatus-v3+json";
                headers!["Content-Type"] = "application/vnd.wirelesscar.ngtp.if9.StartServiceConfiguration-v3+json";
                return this.__Post("swu", headers, swu_data);
            }

            public JsonObject? EnableProvisioningMode(string pin)
            {
                // Enable provisioning mode
                return this._ProvCommand(pin, null, "provisioning");
            }

            public JsonObject? EnableServiceMode(string pin, long expiration_time)
            {
                // Enable service mode. Will disable at the specified time (epoch millis)
                return this._ProvCommand(pin, expiration_time.ToString(), "protectionStrategy_serviceMode");
            }

            public JsonObject? EnableServiceMode(string pin, DateTime expiration_time)
            {
                // Enable service mode. Will disable at the specified time 
                long exp = (long)(expiration_time.ToUniversalTime() - new DateTime(1970, 1, 1)).TotalMilliseconds;
                return this.EnableServiceMode(pin, exp);
            }

            public JsonObject? DisbleServiceMode(string pin)
            {
                // Disable service mode
                long exp = (long)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalMilliseconds;
                return this._ProvCommand(pin, exp.ToString(), "protectionStrategy_serviceMode");
            }

            public JsonObject? EnableGuardianMode(string pin, long expiration_time)
            {
                // Enable Guardian mode. Will be disabled at the specified time (epoch millis)
                return this._GMCommand(pin, expiration_time, "ACTIVATE");
            }

            public JsonObject? EnableGuardianMode(string pin, DateTime expiration_time)
            {
                // Enable Guardian mode. Will be disabled at the specified time 
                long exp = (long)(expiration_time.ToUniversalTime() - new DateTime(1970, 1, 1)).TotalMilliseconds;
                return this.EnableGuardianMode(pin, exp);
            }

            public JsonObject? DisableGuardianMode(string pin)
            {
                // Disable Guardian mode
                return this._GMCommand(pin, 0, "DEACTIVATE");
            }

            public JsonObject? EnableTransportMode(string pin, long expiration_time)
            {
                // Enable transport mode. Will be disabled at the specified time (epoch millis)
                return this._ProvCommand(pin, expiration_time.ToString(), "protectionStrategy_transportMode");
            }

            public JsonObject? EnableTransportMode(string pin, DateTime expiration_time)
            {
                // Enable transport mode. Will be disabled at the specified time 
                long exp = (long)(expiration_time.ToUniversalTime() - new DateTime(1970, 1, 1)).TotalMilliseconds;
                return this.EnableTransportMode(pin, exp);
            }

            public JsonObject? DisableTransportMode(string pin)
            {
                // Disable transport mode
                return this._ProvCommand(pin, null, "protectionStrategy_transportMode");
            }

            public JsonObject? EnablePrivacyMode(string pin)
            {
                // Enable privacy mode. Will disable journey logging
                return this._ProvCommand(pin, null, "privacySwitch_on");
            }

            public JsonObject? DisablePrivacyMode(string pin)
            {
                // Disable privacy mode. Will enable journey logging
                return this._ProvCommand(pin, null, "privacySwitch_off");
            }

            private JsonObject? _ProvCommand(string pin, string? expiration_time, string mode)
            {
                // Send prov endpoint commands. Used for service/transport/privacy mode
                JsonObject? headers = this.connection!.head!.Copy();
                headers!["Content-Type"] = "application/vnd.wirelesscar.ngtp.if9.StartServiceConfiguration-v3+json";
                JsonObject? prov_data = this._AuthenticatePROV(pin);

                prov_data!["serviceCommand"] = mode;
                prov_data!["startTime"] = null;
                prov_data!["endTime"] = expiration_time!;

                return this.__Post("prov", headers, prov_data);
            }

            private JsonObject? _GMCommand(string pin, long expiration_time, string action)
            {
                // Send GM toggle command
                JsonObject? headers = this.connection!.head!.Copy();
                headers!["Accept"] = "application/vnd.wirelesscar.ngtp.if9.GuardianAlarmList-v1+json";
                JsonObject? gm_data = this._AuthenticateGM(pin);
                if (action.Equals("ACTIVATE"))
                {
                    gm_data!["endTime"] = expiration_time;
                    gm_data!["status"] = "ACTIVE";
                    return this.__Post("gm/alarms", headers, gm_data);
                }
                if (action.Equals("DEACTIVATE"))
                {
                    headers!["X-servicetoken"] = gm_data!["token"]!.ToString();
                    return this.__Delete("gm/alarms/INSTANT", headers);
                }
                return null;
            }

            private JsonObject? _AuthenticateVHS()
            {
                // Authenticate to vhs and get token
                return this._AuthenticateEmptyPinProtectedService("VHS");
            }

            private JsonObject? _AuthenticateEmptyPinProtectedService(string service_name)
            {
                return this._AuthenticateService("", service_name);
            }

            private JsonObject? _AuthenticateHBLF()
            {
                // Authenticate to hblf
                return this._AuthenticateVinProtectedService("HBLF");
            }
            private JsonObject? _AuthenticateECC()
            {
                // Authenticate to ecc
                return this._AuthenticateVinProtectedService("ECC");
            }
            private JsonObject? _AuthenticateCP()
            {
                // Authenticate to cp
                return this._AuthenticateVinProtectedService("CP");
            }
            private JsonObject? _AuthenticateSWU()
            {
                // Authenticate to swu
                return this._AuthenticateEmptyPinProtectedService("SWU");
            }

            private JsonObject? _AuthenticateVinProtectedService(string service_name)
            {
                // Authenticate to specified service and return associated token
                return this._AuthenticateService(this.vin!.RightString(4), service_name);
            }

            private JsonObject? _AuthenticateRDL(string pin)
            {
                // Authenticate to rdl
                return this._AuthenticateService(pin, "RDL");
            }

            private JsonObject? _AuthenticateRDU(string pin)
            {
                // Authenticate to rdu
                return this._AuthenticateService(pin, "RDU");
            }

            private JsonObject? _AuthenticateALOFF(string pin)
            {
                // Authenticate to aloff
                return this._AuthenticateService(pin, "ALOFF");
            }

            private JsonObject? _AuthenticateREON(string pin)
            {
                // Authenticate to reon
                return this._AuthenticateService(pin, "REON");
            }

            private JsonObject? _AuthenticateREOFF(string pin)
            {
                // Authenticate to reoff
                return this._AuthenticateService(pin, "REOFF");
            }

            private JsonObject? _AuthenticatePROV(string pin)
            {
                // Authenticate to PROV service
                return this._AuthenticateService(pin, "PROV");
            }

            private JsonObject? _AuthenticateGM(string pin)
            {
                // Authenticate to GM service
                return this._AuthenticateService(pin, "GM");
            }

            private JsonObject? _AuthenticateService(string pin, string service_name)
            {
                // Authenticate to specified service with the provided PIN
                JsonObject data = new JsonObject()
                {
                    { "serviceName", service_name },
                    { "pin", pin}
                };
                JsonObject? headers = this.connection!.head!.Copy();
                headers!["Content-Type"] = "application/vnd.wirelesscar.ngtp.if9.AuthenticateRequest-v2+json";
                return this.__Post($"users/{this.connection.user_id}/authenticate", headers, data);
            }

            private JsonObject? __Get(string command, JsonObject headers)
            {
                // Utility command to get vehicle data from API
                return this.connection!.__Get(command, $"{this.connection.base_urls["IF9"]}/vehicles/{this.vin}", headers);
            }

            private JsonObject? __Post(string command, JsonObject headers, JsonObject? data)
            {
                // Utility command to post data to VHS
                return this.connection!.__Post(command, $"{this.connection.base_urls["IF9"]}/vehicles/{this.vin}", headers, data);
            }

            private JsonObject? __Delete(string command, JsonObject headers)
            {
                // Utility command to delete active service entry
                return this.connection!.__Delete(command, $"{this.connection.base_urls["IF9"]}/vehicles/{this.vin}", headers);
            }

        }


    }



}

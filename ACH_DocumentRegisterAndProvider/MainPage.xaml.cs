using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using System.Reflection;
using Hl7.Fhir.Rest;

namespace ACH_DocumentRegisterAndProvider
{
    public partial class MainPage : ContentPage
    {

        public MainPage()
        {
            InitializeComponent();
            genderPicker.ItemsSource = new List<string> { "Male", "Female", "Other" };
        }

        private async void SubmitButton_Clicked(object sender, EventArgs e)
        {
            string birthdate = dateOfBirthPicker.Date.ToString("yyyy-MM-dd");

            var patient = new Patient
            {
                Name = new List<HumanName> { new HumanName { Given = new[] { nameEntry.Text }, Family = surNameEntry.Text } },
                Gender = genderPicker.SelectedItem.ToString() switch
                {
                    "Male" => AdministrativeGender.Male,
                   "Female" => AdministrativeGender.Female,
                   "Other" => AdministrativeGender.Other,
                  _ => AdministrativeGender.Unknown
                },
                BirthDate = dateOfBirthPicker.Date.ToString("yyyy-MM-dd"),
                Address = new List<Address> { new Address { Text = addressEntry.Text } },
                Telecom = new List<ContactPoint>
                    {
                        new ContactPoint { System = ContactPoint.ContactPointSystem.Phone, Value = phoneEntry.Text },
                      new ContactPoint { System = ContactPoint.ContactPointSystem.Email, Value = emailEntry.Text }
                   },
            };


            // Serialize the patient object to FHIR JSON
            var fhirJsonSerializer = new FhirJsonSerializer();
            string patientJson = fhirJsonSerializer.SerializeToString(patient);

            // Send data to FHIR server
            await SendDataToFhirServer(patientJson, "Patient");
        }
        private async void ViewPatients_Clicked(object sender, EventArgs e)
        {
            try
            {
                var patients = await GetAllPatients();
                if (patients != null)
                {
                    // Assuming you have a ListView or CollectionView named PatientsListView
                    PatientsListView.ItemsSource = patients;
                    PatientsListView.ItemSelected += OnPatientSelected;
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
            }
        }

        private async Task<List<Patient>> GetAllPatients()
        {
            var client = new FhirClient("http://54.93.124.150:8080/fhir/");
            try
            {
                // This is a basic example, in reality, you'll need to handle pagination
                var bundle = await client.SearchAsync<Patient>();
                return bundle.Entry.Select(entry => entry.Resource as Patient).ToList();
            }
            catch (FhirOperationException ex)
            {
                Console.WriteLine("Error retrieving patients: " + ex.Message);
                return null;
            }
        }

        private void OnPatientSelected(object sender, SelectedItemChangedEventArgs e)
        {
            if (e.SelectedItem is Patient patient)
            {
                // Navigate to the patient details page
                Navigation.PushAsync(new PatientDetailPage(patient));
            }
        }

        private async System.Threading.Tasks.Task SendDataToFhirServer(string jsonData, string resourceType)
        {
            using (var client = new HttpClient())
            {
                // Update with the provided FHIR server address
                client.BaseAddress = new Uri("http://54.93.124.150:8080/fhir/");

                var content = new StringContent(jsonData, Encoding.UTF8, "application/fhir+json");

                // Send a POST request to the FHIR server
                var response = await client.PostAsync(resourceType, content);

                if (response != null)
                {
                    // If the response contains an ID, it indicates success
                    await DisplayAlert("Success", "Patient data successfully submitted. ID: " + response, "OK");
                }
                else
                {
                    // If there's no ID in the response, it indicates an issue with the submission
                    await DisplayAlert("Error", "Data submission failed.", "OK");
                }
            }
        }

   
        
    }
}

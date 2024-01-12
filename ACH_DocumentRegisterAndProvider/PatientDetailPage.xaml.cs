using Hl7.Fhir.Model;
using System.Xml;

namespace ACH_DocumentRegisterAndProvider;

public partial class PatientDetailPage : ContentPage
{

    private Patient _patient;

    public PatientDetailPage(Patient patient)
    {
        InitializeComponent();
        _patient = patient;
        LoadPatientDetails();
    }

    private void LoadPatientDetails()
    {
        NameLabel.Text = "Name: " + string.Join(" ", _patient.Name.FirstOrDefault()?.Given) + " " + _patient.Name.FirstOrDefault()?.Family;
        GenderLabel.Text = "Gender: " + _patient.Gender.ToString();
        BirthDateLabel.Text = "Birth Date: " + _patient.BirthDate;
        AddressLabel.Text = "Address: " + _patient.Address.FirstOrDefault()?.Text;
        PhoneLabel.Text = "Phone: " + _patient.Telecom.FirstOrDefault(t => t.System == ContactPoint.ContactPointSystem.Phone)?.Value;
        EmailLabel.Text = "Email: " + _patient.Telecom.FirstOrDefault(t => t.System == ContactPoint.ContactPointSystem.Email)?.Value;

    }

}
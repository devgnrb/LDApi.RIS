using LDApi.RIS.Dto;
using LDApi.RIS.Interfaces;
using LDApi.RIS.Services;
using System.Buffers.Text;
using System.Security.Cryptography;
using LDApi.RIS.Utils;

public class Hl7ServiceTests
{
    private class FixedDateTimeProvider : IDateTimeProvider
    {
        public DateTime Now { get; } = new DateTime(2025, 11, 7, 14, 30, 0);
    }

    private class FixedGuidProvider : IGuidProvider
    {
        public Guid NewGuid() => Guid.Parse("12345678-1234-1234-1234-1234567890ab");
    }

    [Fact]
    public void GenerateHL7Message_ShouldGenerateExactMessageControlIdAndDate()
    {
        // Arrange
        var dateProvider = new FixedDateTimeProvider();
        var guidProvider = new FixedGuidProvider();
        var service = new HL7Service(dateProvider, guidProvider);

        var dto = new ReportDto
        {
            IdReport = 12345,
            FirstName = "John",
            LastName = "Doe",
            DateOfBirth = "19800101",
            TypeDocument = "RAD",
            DateReport = "20251107143000",
            Path = Path.GetTempFileName(),
            EnvoiHL7 = ""
        };

        File.WriteAllText(dto.Path, "PDFSIMULATEDCONTENT");

        string targetApp = "TargetApp";
        string targetFacility = "TargetFacility";

        // Act
        string hl7Message = service.GenerateHL7Message(dto, targetApp, targetFacility);
    
        // Assert

        Assert.Contains($"RPA^R33|1234567812|P|2.3", hl7Message);
        Assert.Contains($"PID|1||{dto.IdReport}||{dto.LastName}^{dto.FirstName}||{dto.DateOfBirth:yyyyMMdd}", hl7Message);
        Assert.Contains($"TXA|1|PN|{dto.TypeDocument}|{dto.DateReport:yyyyMMddHHmmss}", hl7Message);
        Assert.Contains($"OBX|1|ED|PDF^Base64^HL7|1|^UERGU0lNVUxBVEVEQ09OVEVOVA==||||||F", hl7Message);

        // Clean up
        File.Delete(dto.Path);
    }

    [Fact]
    public void GenerateHL7Message_ShouldGenerateValidStructureAndFields()
    {
        // Arrange
        var dateProvider = new FixedDateTimeProvider();
        var guidProvider = new FixedGuidProvider();
        var service = new HL7Service(dateProvider, guidProvider);

        var dto = new ReportDto
        {
            IdReport = 12345,
            FirstName = "John",
            LastName = "Doe",
            DateOfBirth = "19800101",
            TypeDocument = "Laximétrie Dynamique",
            DateReport = "20251107143000",
            Path = Path.GetTempFileName(),
            EnvoiHL7 = ""
        };

        File.WriteAllText(dto.Path, "PDFSIMULATEDCONTENT");

        string targetApp = "TargetApp";
        string targetFacility = "TargetFacility";

        // Act
        string hl7Message = service.GenerateHL7Message(dto, targetApp, targetFacility);

        // Assert — Vérification de la structure
        Assert.False(string.IsNullOrWhiteSpace(hl7Message));
        Assert.Contains("MSH", hl7Message);
        Assert.Contains("PID", hl7Message);
        Assert.Contains("TXA", hl7Message);
        Assert.Contains("OBX", hl7Message);

        // Découpage du message HL7
        var segments = hl7Message.Split('\r', '\n')
                                 .Where(s => !string.IsNullOrWhiteSpace(s))
                                 .ToArray();



        // ---- MSH ----
        var msh = FileHelper.GetField(segments,"MSH", 0);
        Assert.Equal("MSH", msh);

        // Vérifie que le séparateur de champ est bien "|"
        Assert.True(hl7Message.Contains("|"), "Le séparateur de champ MSH-1 doit être '|'.");

        // Test sur la validité de l'ID de contrôle du message MSH-10
        string msh10 = FileHelper.GetField(segments, "MSH", 9);
        // MSH-10 non vide et format valide
        Assert.False(string.IsNullOrEmpty(msh10), "MSH-10 ne doit pas être vide.");
        Assert.True(FileHelper.ValidateMessageControlId(msh10), $"MSH-10 invalide : {msh10}");

        // MSH-9: Message Type (RPA^R33)
        string mshRpa = FileHelper.GetSubField(segments, "MSH", 8, 1);
        string mshR33 = FileHelper.GetSubField(segments, "MSH", 8, 2);
        Assert.Equal("RPA", mshRpa);
        Assert.Equal("R33", mshR33);

        // ---- PID ----
        string pidFamilyName = FileHelper.GetSubField(segments, "PID", 5, 1);
        string pidGivenName = FileHelper.GetSubField(segments, "PID", 5, 2);
        Assert.Equal("Doe", pidFamilyName);
        Assert.Equal("John", pidGivenName);

        var pid7 = FileHelper.GetField(segments, "PID", 7); // Date of birth
        Assert.Equal(dto.DateOfBirth, pid7);

        // ---- TXA ----
        var txa3 = FileHelper.GetField(segments, "TXA", 3); // Document Type
        Assert.Equal(dto.TypeDocument, txa3);

        var txa4 = FileHelper.GetField(segments, "TXA", 4); // Date/Time
        Assert.Equal(dto.DateReport, txa4);

        // ---- OBX ----
        string obxIdentifier = FileHelper.GetSubField(segments, "OBX", 3,1); // Observation Identifier
        string obxText = FileHelper.GetSubField(segments, "OBX", 3, 2);
        string obxNameOfCodingSystem = FileHelper.GetSubField(segments, "OBX", 3, 3);
        Assert.Equal("PDF", obxIdentifier);
        Assert.Equal("Base64", obxText);
        Assert.Equal("HL7", obxNameOfCodingSystem);

        string obxEncoding = FileHelper.GetSubField(segments, "OBX", 5,2); // Value Type
        Assert.Equal("UERGU0lNVUxBVEVEQ09OVEVOVA==", obxEncoding); // Base64 attendu

    


        // Clean up
        File.Delete(dto.Path);
    }


}

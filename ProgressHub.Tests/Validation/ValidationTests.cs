
using FluentAssertions;
using ProgressHub.Core.Validation;
namespace ProgressHub.Tests.Validation
{
    public class ValidationTests
    {

        [Theory]
        [InlineData("demo123@demo.cz")]
        [InlineData("demo@123demo.com")]
        [InlineData("123demo123@1demo1.si")]
        [InlineData("jan.novak@fitness-hub.co.uk")] // Subdomény a pomlčky v doméně
        [InlineData("client+gym@gmail.com")]        // Plus addressing (běžné u Gmailu)
        [InlineData("first.last@domain.travel")]    // Delší TLD domény (.travel, .fitness)
        public void EmailIsValid(string email)
        {   
          
            bool isValid = EmailValidator.IsValid(email);
          
            isValid.Should().BeTrue();
        }

        [Theory]
        [InlineData(null)]                     // Null hodnota
        [InlineData("")]                       // Prázdný řetězec
        [InlineData("   ")]                    // Pouze mezery
        [InlineData("demo123.cz")]             // Chybí @
        [InlineData("@.cz")]                   // Chybí jméno i doména
        [InlineData("demo123@")]               // Chybí celá doména
        [InlineData("demo123@seznam.")]        // Chybí TLD za tečkou
        [InlineData("demo123@seznam")]         // Chybí tečka a TLD
        [InlineData("jan novak@seznam.cz")]    // Mezera uvnitř adresy
        [InlineData("jan@@seznam.cz")]         // Dva zavináče
        [InlineData("jan@seznam..cz")]         // Dvě tečky po sobě v doméně
        [InlineData("@seznam.cz")]             // Chybí jméno uživatele
        [InlineData("jan@.cz")]                // Chybí název domény před TLD
        public void EmailIsInvalid(string email)
        {
            bool isInvalid = EmailValidator.IsValid(email);
            isInvalid.Should().BeFalse();
        }

        



    }
}

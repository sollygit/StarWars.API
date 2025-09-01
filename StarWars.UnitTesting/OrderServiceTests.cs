using StarWars.Model;

namespace StarWars.UnitTesting
{
    public class OrderServiceTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void NeedsReview_ShouldBeTrue_WhenTotalExceedsThreshold()
        {
            // Arrange
            var order = new Order { TotalCents = 2500 };

            // Act
            order.NeedsReview = order.TotalCents >= 2000;

            // Assert
            Assert.That(order.NeedsReview, Is.True);
        }

        [Test]
        public void NeedsReview_ShouldBeFalse_WhenTotalIsBelowThreshold()
        {
            // Arrange
            var order = new Order { TotalCents = 1500 };

            // Act
            order.NeedsReview = order.TotalCents >= 2000;

            // Assert
            Assert.That(order.NeedsReview, Is.False);
        }
    }
}
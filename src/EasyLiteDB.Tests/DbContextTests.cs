
using EasyLiteDB.Tests.Contexts;

namespace EasyLiteDB.Tests
{
    public class DbContextTests
    {
        [Fact]
        public void ConfigureDbContext_ShouldNotThrowException_OnValidConfiguration()
        {
            // Arrange
            var dbContext = new ShopDbContext();

            // Act
            var cars = dbContext.CarEntities.ToList();

            // Assert
            Assert.NotNull(cars);
        }

        [Fact]
        public void AddData_ShouldNotThrowException_OnValidConfiguration()
        {
            // Arrange
            var dbContext = new ShopDbContext();

            // Act
            dbContext.CarEntities.Add(new CarEntity
            {
                Name = "Test"
            });

            // Assert
            Assert.True(dbContext.CarEntities.Any());
        }


        [Fact]
        public void SaveData_ShouldNotThrowException_OnValidConfiguration()
        {
            // Arrange
            var dbContext = new ShopDbContext();

            // Act
            dbContext.CarEntities.Add(new CarEntity
            {
                Name = "Test"
            });

            dbContext.CarEntities.SaveChanges();

            // Assert
            Assert.True(dbContext.CarEntities.Any());
        }

        [Fact]
        public void RemoveData_ShouldNotThrowException_OnValidConfiguration()
        {
            // Arrange
            var dbContext = new ShopDbContext();

            if (!dbContext.CarEntities.Any())
            {
                // Act
                dbContext.CarEntities.Add(new CarEntity
                {
                    Name = "Test"
                });

                dbContext.CarEntities.SaveChanges();
            }


            var car = dbContext.CarEntities.FirstOrDefault()!;

            dbContext.CarEntities.Remove(car);

            dbContext.CarEntities.SaveChanges();

            // Assert
            Assert.False(dbContext.CarEntities.Any(x => x._id == car._id));
        }

        [Fact]
        public void UpdateData_ShouldNotThrowException_OnValidConfiguration()
        {
            // Arrange
            var dbContext = new ShopDbContext();

            if (!dbContext.CarEntities.Any())
            {
                // Act
                dbContext.CarEntities.Add(new CarEntity
                {
                    Name = "Test"
                });

                dbContext.CarEntities.SaveChanges();
            }

            var car = dbContext.CarEntities.FirstOrDefault()!;

            car.Name = "Test2";
            car.SetDirty();

            dbContext.CarEntities.SaveChanges();

            // Assert
            Assert.True(dbContext.CarEntities.Any(x => x._id == car._id && x.Name == "Test2"));
        }

        [Fact]
        public void QueryData_ShouldNotThrowError_OnValidConfiguration()
        {
            // Arrange
            var dbContext = new ShopDbContext();

            // Act
            var cars = dbContext.CarEntities
                .UseQuery(query =>
                {
                    query = query.Where(x => x.Name != null);
                })
                .ToList();

            // Assert
            Assert.NotNull(cars);
        }

        [Fact]
        public void QueryData_ShouldHaveSomeData_OnValidConfiguration()
        {
            // Arrange
            var dbContext = new ShopDbContext();

            // Act
            var cars = dbContext.CarEntities
                .UseQuery(query =>
                {
                    query = query.Where(x => x._id != null);
                })
                .ToList();

            // Assert
            Assert.True(cars.Any());
        }

        [Fact]
        public void QueryData_ShouldNotHaveBadData_WhenDataWithNotFilter()
        {
            // Arrange
            var dbContext = new ShopDbContext();

            var myCar = new CarEntity
            {
                Name = "Bad"
            };

            dbContext.CarEntities.Add(myCar);
            dbContext.CarEntities.SaveChanges();

            // Act
            var cars = dbContext.CarEntities
                .UseQuery(query =>
                {
                    query = query.Where(x => x.Name == "not bad");
                })
                .ToList();

            // Assert
            Assert.DoesNotContain(cars, x => x._id == myCar._id);
        }

        [Fact]
        public void QueryData_ShouldHaveGoodData_WhenDataWithFilter()
        {
            // Arrange
            var dbContext = new ShopDbContext();

            var myCar = new CarEntity
            {
                Name = "Good"
            };
            dbContext.CarEntities.Add(myCar);
            dbContext.CarEntities.SaveChanges();

            // Act
            var cars = dbContext.CarEntities
                .UseQuery(query =>
                {
                    query = query.Where(x => x.Name == "Good");
                })
                .ToList();

            // Assert
            Assert.Contains(cars, x => x._id == myCar._id);
        }
    }
}
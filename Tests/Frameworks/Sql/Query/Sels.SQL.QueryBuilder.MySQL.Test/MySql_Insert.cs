using Sels.Core.Testing.Models;
using System;

namespace Sels.SQL.QueryBuilder.MySQL.Test
{
    public class MySql_Insert
    {
        [Test]
        public void BuildsCorrectInsertQuery()
        {
            // Arrange
            var expected = "INSERT INTO `Person` (`Id`, `Name`, `SurName`)".GetWithoutWhitespace().ToLower();
            var builder = MySql.Insert<Person>().Into().Column(x => x.Id).Column(x => x.Name).Column(x => x.SurName);

            // Act
            var query = builder.Build();

            // Assert
            Assert.IsNotNull(query);
            Assert.AreEqual(expected, query.GetWithoutWhitespace().ToLower());
        }

        [Test]
        public void BuildsCorrectInsertQueryWithMultipleValues()
        {
            // Arrange
            var expected = "INSERT INTO `Person` (`Id`, `Name`, `SurName`) VALUES (1, 'Jens', 'Sels'), (2, 'Jarno', 'Sels')".GetWithoutWhitespace().ToLower();
            var builder = MySql.Insert<Person>().Into().Column(x => x.Id).Column(x => x.Name).Column(x => x.SurName)
                            .Values(1, "Jens", "Sels")
                            .ValuesUsing(new Person() { Id = 2, Name = "Jarno", SurName = "Sels" }, nameof(Person.BirthDay), nameof(Person.ResidenceId));

            // Act
            var query = builder.Build();

            // Assert
            Assert.IsNotNull(query);
            Assert.AreEqual(expected, query.GetWithoutWhitespace().ToLower());
        }

        [Test]
        public void BuildsCorrectInsertQueryWithMultipleParameterizedValues()
        {
            // Arrange
            var expected = "INSERT INTO `Person` (`Id`, `Name`, `SurName`) VALUES (@Id, @Name, @SurName), (@Id1, @Name1, @SurName1)".GetWithoutWhitespace().ToLower();
            var builder = MySql.Insert<Person>().Into().Column(x => x.Id).Column(x => x.Name).Column(x => x.SurName)
                            .Parameters("Id", "Name", "SurName")
                            .ParametersFrom(1.ToString(), nameof(Person.BirthDay), nameof(Person.ResidenceId));

            // Act
            var query = builder.Build();

            // Assert
            Assert.IsNotNull(query);
            Assert.AreEqual(expected, query.GetWithoutWhitespace().ToLower());
        }

        [Test]
        public void BuildsCorrectInsertQueryWithReturningKeyword()
        {
            // Arrange
            var expected = "INSERT INTO `Person` (`Id`, `Name`, `SurName`) VALUES (1, 'Jens', 'Sels') RETURNING *".GetWithoutWhitespace().ToLower();
            var builder = MySql.Insert<Person>().Into().Column(x => x.Id).Column(x => x.Name).Column(x => x.SurName)
                            .Values(1, "Jens", "Sels")
                            .Returning(x => x.All());

            // Act
            var query = builder.Build();

            // Assert
            Assert.IsNotNull(query);
            Assert.AreEqual(expected, query.GetWithoutWhitespace().ToLower());
        }

        [Test]
        public void BuildsCorrectInsertQueryWithImplicitOnDuplicateKeyUpdate()
        {
            // Arrange
            var expected = "INSERT INTO `Person` (`Id`, `Name`, `SurName`) VALUES (1, 'Jens', 'Sels') ON DUPLICATE KEY UPDATE `Name`=VALUES(`Name`), `SurName`=VALUES(`SurName`)".GetWithoutWhitespace().ToLower();
            var builder = MySql.Insert<Person>().Into().Column(x => x.Id).Column(x => x.Name).Column(x => x.SurName)
                            .Values(1, "Jens", "Sels")
                            .OnDuplicateKeyUpdate(0);

            // Act
            var query = builder.Build();

            // Assert
            Assert.IsNotNull(query);
            Assert.AreEqual(expected, query.GetWithoutWhitespace().ToLower());
        }

        [Test]
        public void BuildsCorrectInsertQueryWithOnDuplicateKeyUpdate()
        {
            // Arrange
            var expected = "INSERT INTO `Person` (`Id`, `Name`, `SurName`) VALUES (1, 'Jens', 'Sels') ON DUPLICATE KEY UPDATE `Name`=VALUES(`Name`), `SurName`='Sels'".GetWithoutWhitespace().ToLower();
            var builder = MySql.Insert<Person>().Into().Column(x => x.Id).Column(x => x.Name).Column(x => x.SurName)
                            .Values(1, "Jens", "Sels")
                            .OnDuplicateKeyUpdate(x => x.Set.Column(x => x.Name).To.Values(x => x.Name).And.Set.Column(x => x.SurName).To.Value("Sels"));

            // Act
            var query = builder.Build();

            // Assert
            Assert.IsNotNull(query);
            Assert.AreEqual(expected, query.GetWithoutWhitespace().ToLower());
        }
    }
}

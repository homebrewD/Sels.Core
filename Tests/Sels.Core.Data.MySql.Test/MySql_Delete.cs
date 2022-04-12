﻿using Sels.Core.Data.SQL.Query;
using Sels.Core.Testing.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sels.Core.Data.MySQL.Test
{
    public class MySql_Delete
    {
        [Test]
        public void BuildsCorrectDeleteQuery()
        {
            // Arrange
            var expected = "DELETE FROM Person".GetWithoutWhitespace().ToLower();
            var builder = MySql.Delete().From("Person");

            // Act
            var query = builder.Build();

            // Assert
            Assert.IsNotNull(query);
            Assert.AreEqual(expected, query.GetWithoutWhitespace().ToLower());
        }

        [Test]
        public void BuildsCorrectDeleteQueryWithTableAlias()
        {
            // Arrange
            var expected = "DELETE P FROM Person P".GetWithoutWhitespace().ToLower();
            var builder = MySql.Delete<Person>().From();

            // Act
            var query = builder.Build();

            // Assert
            Assert.IsNotNull(query);
            Assert.AreEqual(expected, query.GetWithoutWhitespace().ToLower());
        }

        [Test]
        public void BuildsCorrectDeleteQueryWithJoin()
        {
            // Arrange
            var expected = "DELETE P FROM Person P FULL JOIN Residence R ON R.Id = P.ResidenceId WHERE P.Id = @Id".GetWithoutWhitespace().ToLower();
            var builder = MySql.Delete<Person>().From()
                                .Join<Residence>(Joins.Full, x => x.On<Residence>(x => x.Id).To(x => x.ResidenceId))
                                .Where(x => x.Column(x => x.Id).EqualTo().Parameter(x => x.Id));

            // Act
            var query = builder.Build();

            // Assert
            Assert.IsNotNull(query);
            Assert.AreEqual(expected, query.GetWithoutWhitespace().ToLower());
        }

        [Test]
        public void BuildsCorrectDeleteQueryWithCondition()
        {
            // Arrange
            var expected = "DELETE P FROM Person P WHERE P.Name LIKE '%Sels%'".GetWithoutWhitespace().ToLower();
            var builder = MySql.Delete<Person>().From()
                                .Where(x => x.Column(x => x.Name).Like("%Sels%"));

            // Act
            var query = builder.Build();

            // Assert
            Assert.IsNotNull(query);
            Assert.AreEqual(expected, query.GetWithoutWhitespace().ToLower());
        }
    }
}

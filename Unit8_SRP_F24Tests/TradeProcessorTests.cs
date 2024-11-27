using Microsoft.Data.SqlClient;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SingleResponsibilityPrinciple;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SingleResponsibilityPrinciple.Tests
{
    [TestClass()]
    public class TradeProcessorTests
    {
        private int CountDbRecords()
        {
            string simbaConnectionString = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\Users\szvidzwa\Documents\tradedatabase.mdf;Integrated Security=True;Connect Timeout=30";
            // Change the connection string used to match the one you want
            using (var connection = new SqlConnection(simbaConnectionString))
            {
                if (connection.State == ConnectionState.Closed)
                {
                    connection.Open();
                }
                string myScalarQuery = "SELECT COUNT(*) FROM trade";
                SqlCommand myCommand = new SqlCommand(myScalarQuery, connection);
                //myCommand.Connection.Open();
                int count = (int)myCommand.ExecuteScalar();
                connection.Close();
                return count;
            }
        }
        [TestMethod()]
        public void TestNormalFile()
        {
            //Arrange
            var tradeStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Unit8_SRP_F24Tests.goodtrades.txt");
            var tradeProcessor = new TradeProcessor();

            //Act
            int countBefore = CountDbRecords();
            tradeProcessor.ProcessTrades(tradeStream);
            //Assert
            int countAfter = CountDbRecords();
            Assert.AreEqual(countBefore + 4, countAfter);
        }
        [TestMethod()]
        public void ProcessTrades_OneGoodTrade_AddsOneRecord()
        {
            // Arrange
            var tradeStream = new MemoryStream(Encoding.UTF8.GetBytes("GBPUSD,1000,1.51"));
            var tradeProcessor = new TradeProcessor();

            // Act
            int countBefore = CountDbRecords();
            tradeProcessor.ProcessTrades(tradeStream);
            int countAfter = CountDbRecords();

            // Assert
            Assert.AreEqual(countBefore + 1, countAfter);
        }
        [TestMethod()]
        public void ProcessTrades_TenGoodTrades_AddsTenRecords()
        {
            // Arrange
            var trades = string.Join(Environment.NewLine, Enumerable.Repeat("GBPUSD,1000,1.51", 10));
            var tradeStream = new MemoryStream(Encoding.UTF8.GetBytes(trades));
            var tradeProcessor = new TradeProcessor();

            // Act
            int countBefore = CountDbRecords();
            tradeProcessor.ProcessTrades(tradeStream);
            int countAfter = CountDbRecords();

            // Assert
            Assert.AreEqual(countBefore + 10, countAfter);
        }
        [TestMethod()]
        public void ProcessTrades_EmptyFile_NoRecordsAdded()
        {
            // Arrange
            var tradeStream = new MemoryStream(Encoding.UTF8.GetBytes(string.Empty));
            var tradeProcessor = new TradeProcessor();

            // Act
            int countBefore = CountDbRecords();
            tradeProcessor.ProcessTrades(tradeStream);
            int countAfter = CountDbRecords();

            // Assert
            Assert.AreEqual(countBefore, countAfter);
        }
        [TestMethod()]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ProcessTrades_NullStream_ThrowsException()
        {
            // Arrange
            Stream tradeStream = null;
            var tradeProcessor = new TradeProcessor();

            // Act
            tradeProcessor.ProcessTrades(tradeStream);

            // Assert handled by ExpectedException
        }
        [TestMethod()]
        public void ProcessTrades_BadTrade_TooManyValues_NoRecordsAdded()
        {
            // Arrange
            var tradeStream = new MemoryStream(Encoding.UTF8.GetBytes("GBPUSD,1000,1.51,EXTRA"));
            var tradeProcessor = new TradeProcessor();

            // Act
            int countBefore = CountDbRecords();
            tradeProcessor.ProcessTrades(tradeStream);
            int countAfter = CountDbRecords();

            // Assert
            Assert.AreEqual(countBefore, countAfter);
        }
        }

    }

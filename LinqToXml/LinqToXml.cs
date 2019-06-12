using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.IO;

namespace LinqToXml
{
    public static class LinqToXml
    {
        /// <summary>
        /// Creates hierarchical data grouped by category
        /// </summary>
        /// <param name="xmlRepresentation">Xml representation (refer to CreateHierarchySourceFile.xml in Resources)</param>
        /// <returns>Xml representation (refer to CreateHierarchyResultFile.xml in Resources)</returns>
        public static string CreateHierarchy(string xmlRepresentation)
        {
            return new XElement("Root", XElement.Parse(xmlRepresentation)
                                                .Elements("Data").GroupBy(x => (string)x
                                                .Element("Category"))
                                                .Select(x => new XElement("Group", new XAttribute("ID", x.Key),
                                                x.Select(y => new XElement("Data", y.Element("Quantity"),
                                                y.Element("Price")))))).ToString();
        }

        /// <summary>
        /// Get list of orders numbers (where shipping state is NY) from xml representation
        /// </summary>
        /// <param name="xmlRepresentation">Orders xml representation (refer to PurchaseOrdersSourceFile.xml in Resources)</param>
        /// <returns>Concatenated orders numbers</returns>
        /// <example>
        /// 99301,99189,99110
        /// </example>
        public static string GetPurchaseOrders(string xmlRepresentation)
        {
            XNamespace aw = "http://www.adventure-works.com";
            XElement root = XElement.Parse(xmlRepresentation);
            var Orders = root.Elements(aw + "PurchaseOrder")
                             .Where(x => x.Elements(aw + "Address")
                             .Any(y => y.Attribute(aw + "Type").Value == "Shipping" && 
                             y.Element(aw + "State").Value == "NY"))
                             .Select(z => z.Attribute(aw + "PurchaseOrderNumber").Value);
            return string.Join(",", Orders);    
        }

        /// <summary>
        /// Reads csv representation and creates appropriate xml representation
        /// </summary>
        /// <param name="customers">Csv customers representation (refer to XmlFromCsvSourceFile.csv in Resources)</param>
        /// <returns>Xml customers representation (refer to XmlFromCsvResultFile.xml in Resources)</returns>
        public static string ReadCustomersFromCsv(string customers)
        {
            string[] str = customers.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            var List = new XElement("Root", str.Select(x => x.Split(','))
                                        .Select(y => new XElement("Customer", 
                                                            new XAttribute("CustomerID", y[0]),
                                                            new XElement("CompanyName", y[1]),
                                                            new XElement("ContactName", y[2]),
                                                            new XElement("ContactTitle", y[3]),
                                                            new XElement("Phone", y[4]),
                                                            new XElement("FullAddress",
                                                                                      new XElement("Address", y[5]),
                                                                                      new XElement("City", y[6]),
                                                                                      new XElement("Region", y[7]),
                                                                                      new XElement("PostalCode", y[8]),
                                                                                      new XElement("Country", y[9]))))).ToString();
            return List;
        }

        /// <summary>
        /// Gets recursive concatenation of elements
        /// </summary>
        /// <param name="xmlRepresentation">Xml representation of document with Sentence, Word and Punctuation elements. (refer to ConcatenationStringSource.xml in Resources)</param>
        /// <returns>Concatenation of all this element values.</returns>
        public static string GetConcatenationString(string xmlRepresentation)
        {
            XElement root = XElement.Parse(xmlRepresentation);
            return root.Value;
        }

        /// <summary>
        /// Replaces all "customer" elements with "contact" elements with the same childs
        /// </summary>
        /// <param name="xmlRepresentation">Xml representation with customers (refer to ReplaceCustomersWithContactsSource.xml in Resources)</param>
        /// <returns>Xml representation with contacts (refer to ReplaceCustomersWithContactsResult.xml in Resources)</returns>
        public static string ReplaceAllCustomersWithContacts(string xmlRepresentation)
        {
            var doc = XElement.Parse(xmlRepresentation);
            doc.ReplaceAll(doc.Elements().Select(x => new XElement("contact", x.Descendants())));
            return doc.ToString();
        }

        /// <summary>
        /// Finds all ids for channels with 2 or more subscribers and mark the "DELETE" comment
        /// </summary>
        /// <param name="xmlRepresentation">Xml representation with channels (refer to FindAllChannelsIdsSource.xml in Resources)</param>
        /// <returns>Sequence of channels ids</returns>
        public static IEnumerable<int> FindChannelsIds(string xmlRepresentation)
        {
            XElement data = XElement.Parse(xmlRepresentation);
            var IDs = data.Elements("channel")
                          .Where(x => x.Elements("subscriber").Count() > 1 && x.Nodes()
                          .OfType<XComment>().Any(y => y.Value == "DELETE"))
                          .Select(x => (int)x.Attribute("id"));
            return IDs;
        }

        /// <summary>
        /// Sort customers in docement by Country and City
        /// </summary>
        /// <param name="xmlRepresentation">Customers xml representation (refer to GeneralCustomersSourceFile.xml in Resources)</param>
        /// <returns>Sorted customers representation (refer to GeneralCustomersResultFile.xml in Resources)</returns>
        public static string SortCustomers(string xmlRepresentation)
        { 
            XElement root = XElement.Parse(xmlRepresentation);
            var sorting = new XElement("Root",root.Elements("Customers")
                              .OrderBy(x => x.Element("FullAddress").Element("Country").Value)
                              .ThenBy(x => x.Element("FullAddress").Element("City").Value)
                              .Select(x => x));
            return sorting.ToString();
        }

        /// <summary>
        /// Gets XElement flatten string representation to save memory
        /// </summary>
        /// <param name="xmlRepresentation">XElement object</param>
        /// <returns>Flatten string representation</returns>
        /// <example>
        ///     <root><element>something</element></root>
        /// </example>
        public static string GetFlattenString(XElement xmlRepresentation)
        {
            XElement doc = XElement.Parse(xmlRepresentation.ToString());
            doc.Save(xmlRepresentation.ToString(), SaveOptions.DisableFormatting);
            return doc.ToString();
        }

        /// <summary>
        /// Gets total value of orders by calculating products value
        /// </summary>
        /// <param name="xmlRepresentation">Orders and products xml representation (refer to GeneralOrdersFileSource.xml in Resources)</param>
        /// <returns>Total purchase value</returns>
        public static int GetOrdersValue(string xmlRepresentation)
        {
            XElement root = XElement.Parse(xmlRepresentation);
            var OrdersValue = root.Elements("Orders").Elements("Order").Select(x => x.Element("product").Value);
            var Products = root.Element("products").Elements();
            return OrdersValue.Join(
                                Products,
                                order => order,
                                product => product.Attribute("Id").Value,
                                (order, product) => (int)product.Attribute("Value")).Sum();
        }

    }
}

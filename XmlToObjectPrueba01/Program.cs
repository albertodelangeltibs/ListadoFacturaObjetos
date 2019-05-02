#region Library
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml;
using System.Reflection;
using XmlToObjectPrueba01.ServiceReference1;
using System.Data;
using System.Text.RegularExpressions;
using System.Collections;
#endregion End of Library

namespace XmlToObjectPrueba01
{
    class Program
    {
        static void Main(string[] args)
        {
        #region Variables - Xml Transformation
            
            // Initialize custom class for xml serialization
            Serializer ser = new Serializer();
            string path = string.Empty;
            string xmlInputData = string.Empty;
            string xmlOutputData = string.Empty;

            // Declare the path for xml file
            path = Directory.GetCurrentDirectory() + @"\Salida.xml";
            xmlInputData = File.ReadAllText(path);

            // Initialize the Class for match xml node with name of class
            Facturas factura = ser.Deserialize<Facturas>(xmlInputData);

            // Initialize listaFactura Object
            List<PropiedadesFactura> listaFact = new List<PropiedadesFactura>();

            //XmlToObjectPrueba01.ServiceReference1.Concepto prueba = new XmlToObjectPrueba01.ServiceReference1.Concepto();
            //prueba.Impuestos.Traslados.TrasladoLista[0].

            #region Loop 1 - Add facturas a la lista
            // Fill the object with values form xml 
            for (int i = 0; i < factura.Propiedades.Count; i++)
            {
                listaFact.Add(factura.Propiedades[i]);
            }
            #endregion END - Loop 1
            
            // Object's list from facturas
            IEnumerable<PropiedadesFactura> filterFacturas1 = listaFact.GroupBy(x => x.FolioABA)
                .Select(y => y.First()).Where(e => e.FolioABA != "0");

            // Object's list from Clases in Service
            List<object> IdConceptos = new List<object>();

            #endregion END Variables
            #region Iteration 1 - Get All Facturas from Xml
            foreach (PropiedadesFactura facturas in filterFacturas1)
            {

                // Object's list from IdGeneral
                IEnumerable<PropiedadesFactura> propertyClassGeneric = listaFact.GroupBy(e => e.IdGeneral)
                .Select(y => y.First()).Where(e => e.FolioABA == facturas.FolioABA);

            #endregion
                #region Iteration 2 - Get All IdGeneral for each Factura in Loop
                foreach (PropiedadesFactura IdGenerales in propertyClassGeneric)
                {

                // Object's list from IdGeneral in each FolioABA
                IEnumerable<PropiedadesFactura> filterIIdGeneral2 = listaFact.Select(
                   e => e).Where(
                   e => e.IdGeneral == IdGenerales.IdGeneral && 
                   e.FolioABA == facturas.FolioABA);

                // Object's list from ValorUnitario - Class Name
                IEnumerable<PropiedadesFactura> ClaseCampoValor = listaFact.Select(
                   e => e).Where(
                   e => e.IdGeneral == IdGenerales.IdGeneral && 
                   e.FolioABA == facturas.FolioABA);

                // Initialize Dictionary and List
                var ClasesName = new Dictionary<String, object>();
                List<string> arrayClases = new List<string>();
                
                // Fill the Dictionary with data - Key = Valor && Value = Dato
                foreach (var Datos in ClaseCampoValor)
                {
                    // Regular expresion by get all characters before dot
                    Regex regex = new Regex(@"^\w*");
                    // Get data for Regular expression
                    Match match = regex.Match(Datos.Valor);

                    if (ClasesName.ContainsKey(match.ToString()))
                    {
                        // Do nothing when the key is repeat
                    }
                    else { 
                        // Add Variables to Dictionary and Array
                        arrayClases.Add(match.ToString());
                        ClasesName.Add(match.ToString(), Datos.Dato);
                    }
                }

                // Object's list from IdConcepto
                IEnumerable<PropiedadesFactura> filterIdConcepto3 = filterIIdGeneral2.GroupBy(x => x.IdConcepto)
                    .Select(y => y.First()).Distinct();
                #endregion
                     #region BEGIN Iteration 3 - Get All IdConcepto for each IdGeneral in Loop
                foreach (PropiedadesFactura createObject in filterIdConcepto3)
                {

                    // Object's list from each IdConcepto in each IdGeneral in each Factura
                    IEnumerable<PropiedadesFactura> objectCreated = filterIIdGeneral2
                        .Where(e => e.FolioABA == facturas.FolioABA &&
                        e.IdConcepto == createObject.IdConcepto);

                    string cl = arrayClases[0].ToString();
                    string clase = cl.Substring(0, 1).ToUpper() + cl.Substring(1);


                    if (clase == "Addenda") 
                    {

                        IEnumerable<PropiedadesFactura> AddendaList = listaFact.Select(
                        e => e).Where(
                        e => e.FolioABA == facturas.FolioABA && e.Addenda != "0");

                        //public static String Addeenda;


                        foreach (var Adenda in AddendaList)
                        {
                            Variables.Addenda = Adenda.Addenda;
                        }

                    }
                  

                    string objectToInstantiate = 
                        "XmlToObjectPrueba01.ServiceReference1." + clase + ","+ " XmlToObjectPrueba01";
                    var CopyClass = Type.GetType(objectToInstantiate);

                   

                    if (CopyClass == null) { continue; }

                    //if (CopyClass == "Concepto")
                    //{
                    //    Console.WriteLine(CopyClass);
                    //}

                    var GenericInstace = Activator.CreateInstance(CopyClass);
                    // Initialize Dictoriony by object's list from data - Concepto
                    var dictionary = new Dictionary<String, object>();

                    #region Create List of objects
                    // Fill the Dictionary with data - Key = Valor && Value = Dato
                    foreach (var Datos in objectCreated)
                    {
                        // Regular expresion by get all characters after dot
                        Regex regex = new Regex(@"\w*$");
                        // Get data for Regular expression
                        Match match = regex.Match(Datos.Valor);
                        // Add Keys and Values
                        dictionary.Add(match.ToString(),Datos.Dato);
                    }
                    #endregion
                    // Get array properties from Class Concepto - Service Reference
                    //PropertyInfo[] properties = typeof(XmlToOb jectPrueba01.ServiceReference1.Concepto)
                    //    .GetProperties();

                    // Get array properties from Class Concepto - Service Reference
                    PropertyInfo[] properties = Type.GetType(objectToInstantiate).GetProperties();
                #endregion END Iteration 4
                          #region Iteration 4 - Set values to Class - Services Reference

                    foreach (PropertyInfo proper in properties)
                    {
                        #region BEGIN - Conditions to Insert values in class
                        // Filters for properties from object Conceptos - Service Reference

                        if (proper.Name == "Addenda"){ proper.SetValue(GenericInstace, Variables.Addenda); }
                        else if (proper.Name == "Impuestos")
                        {
                            Console.WriteLine(proper);
                            // LLAMAR A LA PROPIEDAD DE LA CLASE DENTRO 
                        }
                        else if (proper.PropertyType == typeof(String[])){
                            if (dictionary.ContainsKey(proper.Name.ToString()))
                                {
                                    proper.SetValue(GenericInstace,
                                    new String[] { dictionary[proper.Name].ToString() });
                                }
                            }
                        else if (proper.PropertyType == typeof(String)){
                                if (dictionary.ContainsKey(proper.Name.ToString()))
                                {
                                    proper.SetValue(GenericInstace,
                                    dictionary[proper.Name.ToString()]);
                                }
                            }
                        else if (proper.PropertyType == typeof(Decimal))
                            {
                                if (dictionary.ContainsKey(proper.Name.ToString()))
                                {
                                    proper.SetValue(GenericInstace,
                                    Convert.ToDecimal(dictionary[proper.Name.ToString()]));
                                }
                            }
                        else if (proper.PropertyType == typeof(int))
                            {
                                if (dictionary.ContainsKey(proper.Name.ToString()))
                                {
                                    proper.SetValue(GenericInstace,
                                    Convert.ToInt32(dictionary[proper.Name.ToString()]));
                                }
                                // switch (t.PropertyType.FullName)
                            }
                        #endregion END - Conditions for set values
                    }
                    #endregion END Iteration 4
                    // Add Object to object ListFact
                    IdConceptos.Add(GenericInstace);
                    }
                }
            }
            Console.ReadLine();
        }
        public static class Variables
        {
            public static String Addenda {set; get;}
        }
    }
}

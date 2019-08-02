// Josep Lagunas 28-02-2011

using System.Xml.Serialization;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace Utils
{
    public class SerializationServices
    {
        private static SerializationServices SS;
        private static Object syncLook = new Object();

        private SerializationServices()
        {
        }

        public static SerializationServices GetInstance
        {
            get
            {
                if (SS == null)
                {
                    lock (syncLook)
                    {
                        if (SS == null)
                        {
                            SS = new SerializationServices();
                        }
                    }
                }

                return SS;
            }
        }

        /// <summary>
        /// Retorna un objecte serialitzat en JSON
        /// </summary>
        /// <param name="o">Instància a serialitzar</param>
        /// <returns>String que conté la serialització en json de l'obtecte passat com a paràmetre</returns>
        public string GetJSONfromObject(object o)
        {
            try
            {
                using (MemoryStream m = new MemoryStream())
                {
                    return JsonConvert.SerializeObject(o);
                }
            }
            catch (Exception e)
            {
                throw new Exception("Error serializing to json.", e);
            }
        }

        /// <summary>
        /// Retorna una instància de T a partir d'un objecte serialitzat en JSON
        /// </summary>
        /// <param name="json">Objecte serialitzat en format json</param>
        /// <param name="T">Tipus de la instància a retornar</param>
        /// <returns>Retorna una instància del tipus igual al nom passat com a segon paràmetre amb les dades del objecte serialitzat passat com a primer paràmetre</returns>
        public T GetObjectFromJSON<T>(string json)
        {
            try
            {
                using (MemoryStream m = new MemoryStream(Encoding.UTF8.GetBytes(json)))
                {
                    T obj = JsonConvert.DeserializeObject<T>(json,
                        new JsonSerializerSettings()
                        {
                            NullValueHandling = NullValueHandling.Ignore,
                            StringEscapeHandling = StringEscapeHandling.EscapeHtml,
                            DateFormatHandling = DateFormatHandling.MicrosoftDateFormat
                        });

                    return obj;
                }
            }
            catch (Exception e)
            {
                throw new Exception("Error deserializing from json.", e);
            }
        }

        public bool TryParse<T>(string json, out T item)
        {
            try
            {
                using (MemoryStream m = new MemoryStream(Encoding.UTF8.GetBytes(json)))
                {
                    item = JsonConvert.DeserializeObject<T>(json,
                        new JsonSerializerSettings()
                        {
                            NullValueHandling = NullValueHandling.Ignore,
                            StringEscapeHandling = StringEscapeHandling.EscapeHtml,
                            DateFormatHandling = DateFormatHandling.MicrosoftDateFormat
                        });
                }

                return true;
            }
            catch (Exception)
            {
                item = default(T);
                return false;
            }
        }

        public T GetObjectFromXML<T>(string xmlString)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(xmlString);
            XmlSerializer x = new XmlSerializer(typeof(T));
            return (T) x.Deserialize(PopEOF(buffer));
        }

        public string GetXMLFromObject(object o)
        {
            XmlSerializer x = new XmlSerializer(o.GetType());
            using (MemoryStream str = new MemoryStream())
            {
                x.Serialize(str, o);
                ArraySegment<byte> buffer;
                str.TryGetBuffer(out buffer);
                return Encoding.UTF8.GetString(PutEOF(buffer.Array));
            }
        }

        public byte[] GetBytesFromObject(object o)
        {
            IFormatter f = new BinaryFormatter();
            using (MemoryStream str = new MemoryStream())
            {
                f.Serialize(str, o);
                return str.GetBuffer();
            }
        }

        public T GetObjectFromBytes<T>(byte[] buffer)
        {
            IFormatter f = new BinaryFormatter();
            using (Stream str = new MemoryStream(buffer))
            {
                T o = (T) f.Deserialize(str);
                return o;
            }
        }

        // aquest mètodes afegeixen i treuen un marca de final de transmissió per bloc molt grans en funció de boolean a la crida al mètode serialitzador
        private byte[] PutEOF(byte[] buf)
        {
            byte[] aux = new byte[buf.Length + 5];
            byte[] auxEOF = Encoding.UTF8.GetBytes("<EOF>");

            buf.CopyTo(aux, 0);
            auxEOF.CopyTo(aux, buf.Length);
            return aux;
        }

        private MemoryStream PopEOF(byte[] buf)
        {
            byte[] aux = new byte[buf.Length - 5];
            System.Array.Copy(buf, aux, (buf.Length - 5));
            return new MemoryStream(aux);
        }
    }
}
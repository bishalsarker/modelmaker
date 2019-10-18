using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Reflection;

namespace ModelMaker
{
    public class Model
    {
        private int statusCode { get; set; }
        private SqlConnection conn;

        public Model(string connectionString)
        {
            setConnectionString(connectionString);
        }
        public void Exec(string query)
        {
            if (connOpen())
            {
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.ExecuteNonQuery();
                setStatus(200);
                connClose();
            }
            else
            {
                setStatus(500);
            }
        }
        public List<T> Read<T>(string query, EntityMap entityMap)
        {
            List<T> objList = new List<T>();
            Type objType = typeof(T);

            if (connOpen())
            {
                SqlCommand cmd = new SqlCommand(query, conn);
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    object obj = Activator.CreateInstance(objType);
                    int i = 0;
                    while (i < reader.FieldCount)
                    {
                        string fName = reader.GetName(i);
                        string pName = entityMap.GetProperty(fName);
                        PropertyInfo prop = objType.GetProperty(pName);
                        if (prop != null)
                        {
                            prop.SetValue(obj, reader[fName] + "");
                        }
                        else
                        {
                            prop.SetValue(obj, null);
                        }

                        i++;
                    }
                    objList.Add((T)obj);
                }
                reader.Close();
                setStatus(200);
                connClose();
            }
            else
            {
                setStatus(500);
            }

            return objList;
        }
        public int Count(string query)
        {
            int count = 0;
            if (connOpen())
            {
                SqlCommand cmd = new SqlCommand(query, conn);
                count = (Int32)cmd.ExecuteScalar();
                setStatus(200);
                connClose();
            }
            else
            {
                setStatus(500);
            }
            return count;
        }
        public int getStatus()
        {
            return statusCode;
        }

        private bool connOpen()
        {
            try
            {
                conn.Open();
                return true;
            }
            catch (SqlException ex)
            {
                return false;
            }
        }
        private bool connClose()
        {
            try
            {
                conn.Close();
                return true;
            }
            catch (SqlException ex)
            {
                return false;
            }
        }
        private void setStatus(int code)
        {
            statusCode = code;
        }
        private void setConnectionString(string connStr)
        {
            conn = new SqlConnection(connStr);
        }

    }

    public class EntityMap
    {
        private Dictionary<string, string> map = new Dictionary<string, string>();

        public EntityMap(string[] connections)
        {
            foreach (string conn in connections)
            {
                string[] splits = conn.Split(':');
                string pName = splits[0].Trim();
                string fName = splits[1].Trim();
                AddConnection(pName, fName);
            }
        }

        public void AddConnection(string propertyName, string fieldName)
        {
            string result = "";
            if (!(map.TryGetValue(propertyName, out result)))
            {
                map.Add(propertyName, fieldName);
            }
        }

        public string GetField(string propertyName)
        {
            string result = "";
            if (map.TryGetValue(propertyName, out result))
            {
                return result;
            }
            else
            {
                return null;
            }
        }

        public string GetProperty(string fieldName)
        {
            int i = 0;
            string propName = "";
            while (i < map.Count())
            {
                string pName = map.Keys.ElementAt(i);
                string fName = GetField(pName);
                if (fieldName.Trim().Equals(fName.Trim()))
                {
                    propName = pName;
                    break;
                }
                else
                {
                    i++;
                }
            }

            if (propName != "")
            {
                return propName;
            }
            else
            {
                return null;
            }
        }

        public string GetPropertyAt(int index)
        {
            return map.Keys.ElementAt(index);
        }

        public int Count()
        {
            return map.Count();
        }
    }
}

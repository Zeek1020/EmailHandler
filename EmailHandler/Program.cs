#define SPEED

using EmailHandler.Models;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Resources;
using System.Windows.Forms;
using Newtonsoft.Json;
using System.Collections.Generic;
using EmailHandler.Properties;
using System.Collections;

namespace EmailHandler
{
    internal static class Program
    {
        const string resourcePath = @"../../Properties/Resources.resx";

        static T ReadFiletObject<T>( string path){
            string content = null;
            using (var reader = new StreamReader(File.OpenRead(path)))
            {
                content = reader.ReadToEnd();
            }
            return JsonConvert.DeserializeObject<T>(content);
        }
        
        static Dictionary<string, string> GetResources( string path)
        {
            var output = new Dictionary<string, string>();
            using( ResXResourceReader reader = new ResXResourceReader(path)){
                foreach( DictionaryEntry entry in reader)
                {
                    string key = entry.Key.ToString();
                    string value = entry.Value.ToString();
                    if( output.ContainsKey(key) ){
                        output[key] = value;
                    }
                    else
                    {
                        output.Add(key, value);
                    }
                }
            }
            return output;
        }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main() {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
#if !SPEED

            bool exit = false;
            int count = 0;
            
            while( !exit && count < 10 )
            {
                count++;
                Form1 form = new Form1();
                Application.Run(form);
                
                try{
                    if (form.valid)
                    {
                        MailManagerAsync handler = new MailManagerAsync(form.username, form.password, form.host);
                        Application.Run(new Form2( handler ));
                    }
                } catch ( InvalidOperationException ex ){
                    Debug.WriteLine(ex);
                    continue;
                } catch ( Exception ex ){
                    Debug.WriteLine( ex );
                }
                exit = true;
            }

#else

            
            string UserSecretsKey = null;
            string path = null;

            var resources = GetResources( resourcePath );
            UserSecretsKey = resources["UserSecretsKey"];
            
            path = $@"{Environment.ExpandEnvironmentVariables( "%appdata%" )}\Microsoft\UserSecrets\{UserSecretsKey}\secrets.json";

            var secrets = ReadFiletObject<Dictionary<string, string>>(path);
            string username = secrets["username"] ?? null;
            string password = secrets["password"] ?? null;
            string host = secrets["host"] ?? null;

            MailManagerAsync handler = new MailManagerAsync( username, password, host);
            Application.Run( new Form2( handler ) );
#endif
        }
    }
}
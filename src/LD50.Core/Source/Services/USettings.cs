using Microsoft.Xna.Framework;
using System;
using System.IO;
using System.Xml.Linq;

namespace LD50.Core
{
    public class USettings : IService
    {
        public static string GetPersistentDataFolder()
        {
            string dir = Directory.GetCurrentDirectory();

            if (Directory.Exists(dir) == false)
            {
                Directory.CreateDirectory(dir);
            }

            return dir;
        }

        public int Update()
        {
            return 1;
        }

        public int Update(GameTime gameTime)
        {
            throw new NotImplementedException();
        }

        public USettings()
        {
            XDocument doc;
            try
            {
                doc = XDocument.Load(GetPersistentDataFolder() + "\\SETTINGS.ini");
            }
            catch (Exception)
            {
                //Debug.Log(ex);
                doc = null;
            }

            if (doc != null)
            {
                XElement user = doc.Element("document").Element("user");

                if (user != null)
                {
                    /*
                    SafeParse.ParseFloat(user.Element("walk_speed"), ref Settings.sessionUserWalkSpeed);
                    SafeParse.ParseFloat(user.Element("fly_speed"), ref Settings.sessionUserFlySpeed);
                    SafeParse.ParseFloat(user.Element("rotation_speed"), ref Settings.sessionUserRotationSpeed);
                    SafeParse.ParseFloat(user.Element("height"), ref Settings.sessionUserHeight);

                    Settings.sessionUserWalkSpeed = Mathf.Clamp(Settings.sessionUserWalkSpeed, 1.0f, 50.0f);
                    Settings.sessionUserFlySpeed = Mathf.Clamp(Settings.sessionUserFlySpeed, 1.0f, 50.0f);
                    Settings.sessionUserRotationSpeed = Mathf.Clamp(Settings.sessionUserRotationSpeed, 1.0f, 360.0f);
                    Settings.sessionUserHeight = Mathf.Clamp(Settings.sessionUserHeight, 1.0f, 50.0f);
                    */
                }
                else
                {
                    //Debug.LogWarning("Enable to load from folder");
                }
            }
        }
    }
}

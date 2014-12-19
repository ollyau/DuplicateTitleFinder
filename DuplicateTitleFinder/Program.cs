using iniLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DuplicateTitleFinder {
    class Program {
        static void Main(string[] args) {
            Console.WriteLine(string.Format("Duplicate Title Finder by Orion Lyau\r\nVersion: {0}", System.Reflection.Assembly.GetExecutingAssembly().GetName().Version));
            try {
                Init();
            }
            catch (Exception ex) {
                Console.WriteLine(ex);
            }
            Console.WriteLine("\r\nPress any key to close.");
            Console.ReadKey();
        }

        static void Init() {
            var parser = new ArgumentParser();
            List<Simulator> sims = new List<Simulator>();

            if (!parser.Check("sim", (arg) => {
                if (arg.Equals("fsx", StringComparison.InvariantCultureIgnoreCase)) {
                    sims.Add(new FlightSimulatorX());
                }
                if (arg.Equals("esp", StringComparison.InvariantCultureIgnoreCase)) {
                    sims.Add(new EnterpriseSimulationPlatform());
                }
                else if (arg.Equals("p3d", StringComparison.InvariantCultureIgnoreCase)) {
                    sims.Add(new Prepar3D());
                }
                else if (arg.Equals("p3d2", StringComparison.InvariantCultureIgnoreCase)) {
                    sims.Add(new Prepar3D2());
                }
                else if (arg.Equals("fsxse", StringComparison.InvariantCultureIgnoreCase)) {
                    sims.Add(new FlightSimulatorXSteamEdition());
                }
            })) {
                List<Simulator> allSims = new List<Simulator>();
                allSims.Add(new FlightSimulatorX());
                allSims.Add(new EnterpriseSimulationPlatform());
                allSims.Add(new Prepar3D());
                allSims.Add(new Prepar3D2());
                allSims.Add(new FlightSimulatorXSteamEdition());
                sims = new List<Simulator>(allSims.Where(x => x.Directory != Simulator.NOT_FOUND));
            }

            if (sims.Count == 0) {
                Console.WriteLine("\r\nNo simulators found.");
                return;
            }

            foreach (var sim in sims) {
                Console.WriteLine("\r\nSimulator: {0}", sim.Name);

                var duplicateTitles = DuplicateTitles(new SimConfig(sim).SimObjectDirectories());

                if (duplicateTitles.Count() == 0) {
                    Console.WriteLine("\r\nNo duplicate titles found.");
                }
                else {
                    foreach (var title in duplicateTitles) {
                        Console.WriteLine("\r\n{0}\r\n{1}\r\n", title.Key, string.Join("\r\n", title.ToArray()));
                    }
                }
            }
        }

        private static IEnumerable<IGrouping<string, string>> DuplicateTitles(List<string> directories) {
            string[] filenames = { "aircraft.cfg", "sim.cfg" };
            List<KeyValuePair<string, string>> titleLookup = new List<KeyValuePair<string, string>>();
            foreach (var objectsDir in directories) {
                if (!Directory.Exists(objectsDir)) {
                    continue;
                }
                foreach (var objectDir in Directory.GetDirectories(objectsDir)) {
                    foreach (var name in filenames) {
                        var cfgPath = Path.Combine(objectDir, name);
                        if (!File.Exists(cfgPath)) {
                            continue;
                        }
                        Ini simCfg = new Ini(cfgPath);
                        foreach (string s in simCfg.GetCategoryNames().Where(x => x.StartsWith("fltsim."))) {
                            string title = simCfg.GetKeyValue(s, "title");
                            titleLookup.Add(new KeyValuePair<string, string>(title, cfgPath));
                        }
                    }
                }
            }
            return titleLookup.ToLookup(x => x.Key, x => x.Value).Where(x => x.Count() > 1);
        }
    }
}

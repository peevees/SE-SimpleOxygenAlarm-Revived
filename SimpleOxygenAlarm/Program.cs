using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System;
using VRage.Collections;
using VRage.Game.Components;
using VRage.Game.ModAPI.Ingame;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRage.Game.ObjectBuilders.Definitions;
using VRage.Game;
using VRageMath;

namespace IngameScript
{
    partial class Program : MyGridProgram
    {
        // This file contains your actual script.
        //
        // You can either keep all your code here, or you can create separate
        // code files to make your program easier to navigate while coding.
        //
        // In order to add a new utility class, right-click on your project, 
        // select 'New' then 'Add Item...'. Now find the 'Space Engineers'
        // category under 'Visual C# Items' on the left hand side, and select
        // 'Utility Class' in the main area. Name it in the box below, and
        // press OK. This utility class will be merged in with your code when
        // deploying your final script.
        //
        // You can also simply create a new utility class manually, you don't
        // have to use the template if you don't want to. Just do so the first
        // time to see what a utility class looks like.

        public Program()
        {
            // The constructor, called only once every session and
            // always before any other method is called. Use it to
            // initialize your script. 
            //     
            // The constructor is optional and can be removed if not
            // needed.
            // 
            // It's recommended to set RuntimeInfo.UpdateFrequency 
            // here, which will allow your script to run itself without a 
            // timer block.
        }

        public void Save()
        {
            // Called when the program needs to save its state. Use
            // this method to save your state to the Storage field
            // or some other means. 
            // 
            // This method is optional and can be removed if not
            // needed.
        }

        public void Main(string argument, UpdateType updateSource)
        {
            // The main entry point of the script, invoked every time
            // one of the programmable block's Run actions are invoked,
            // or the script updates itself. The updateSource argument
            // describes where the update came from. Be aware that the
            // updateSource is a  bitfield  and might contain more than 
            // one update type.
            // 
            // The method itself is required, but the arguments above
            // can be removed if not needed.


            // Define the variables        
            var LCD = GetBlocksFromGroup("Oxygen LCD Panels"); // lcd group name: Oxygen LCD Panels
            var airVents = SearchBlocksByName("Air Vent"); // Air vents name: Air Vent        
            var timer = GridTerminalSystem.GetBlockWithName("Timer Block Oxygen") as IMyTimerBlock;//nameoftimerblock   
            var alarm = GridTerminalSystem.GetBlockWithName("Oxygen Alarm") as IMyFunctionalBlock; //nameofsoundblock
            var alarma = GridTerminalSystem.GetBlockWithName("Emergency Lights") as IMyInteriorLight;//nameoflight
            var pressureStatus = "Pressurized";
            string LCDText = " Life Support System:\r\n\r\n";
            var LCDColour = Color.White;

            // Check the air vents        
            for (int i = 0; i < airVents.Count; i++)
            {
                string pressureInfo = airVents[i].DetailedInfo;
                if (pressureInfo.IndexOf("Not pressurized") != -1)
                {
                    if (pressureStatus == "Pressurized")
                    {
                        LCDText += "                ----------- ALERT ----------- \r\n\r\n";
                    }
                    LCDText += airVents[i].CustomName.Substring(8) + " depressurised \r\n";
                    LCDColour = Color.Red;
                    pressureStatus = "Depressurized";
                }
            }//plays sound on sound blocks named Oxygen Alarm             

            if (pressureStatus == "Depressurized")
            {
                alarm.GetActionWithName("PlaySound").Apply(alarm);
                List<IMyTerminalBlock> allDoors = new List<IMyTerminalBlock>();
                GridTerminalSystem.GetBlocksOfType<IMyDoor>(allDoors);
                for (int i = 0; i < allDoors.Count; i++) {
                    allDoors[i].GetActionWithName("Open_Off").Apply(allDoors[i]);
                }
            }
            else
            {
                LCDText += " All " + airVents.Count + " zones are currently pressurised";
                //set lights to off
                //alarma.GetActionWithName("OnOff_Off").Apply(alarma);
            }

            SetText(LCD, LCDText, LCDColour);

            // Restart the event handler        
            timer.GetActionWithName("Start").Apply(timer);

        }

        // Method for finding blocks by names          
        List<IMyTerminalBlock> SearchBlocksByName(string blockName)
        {
            var blocks = new List<IMyTerminalBlock>();
            GridTerminalSystem.SearchBlocksOfName(blockName, blocks);
            return blocks;
        }

        // Method for finding block groups           
        List<IMyTerminalBlock> GetBlocksFromGroup(string group)
        {
            IMyBlockGroup taggedGroup = GridTerminalSystem.GetBlockGroupWithName(group);
            List<IMyTerminalBlock> listGroup = new List<IMyTerminalBlock>();
            List<IMyTerminalBlock> finallist = new List<IMyTerminalBlock>();
            if (taggedGroup != null)
            {
                taggedGroup.GetBlocks(listGroup);
            }

            //check group tagged blocks are of right type 
            for (int i = 0; i < listGroup.Count; i++)
            {
                if (listGroup[i] is IMyTextPanel)//Can be made generic
                    finallist.Add(listGroup[i]);
            }
            if (finallist != null)
            {
                return finallist;
            }
            throw new Exception("GetBlocksFromGroup: Group \"" + group + "\" not found");
        }

        // Method for writting to LCD Panels
        void SetText(List<IMyTerminalBlock> blocks, string LCDText, Color color)
        {
            for (int i = 0; i < blocks.Count; i++)
            {
                IMyTextPanel panel = blocks[i] as IMyTextPanel;
                panel.WritePublicText(LCDText);
                panel.SetValue("FontColor", color);
                panel.ShowTextureOnScreen();
                panel.ShowPublicTextOnScreen();
            }
        }
    }
}

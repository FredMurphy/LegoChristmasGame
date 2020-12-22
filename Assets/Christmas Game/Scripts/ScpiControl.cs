using System;
using UnityEngine;

public class ScpiControl : MonoBehaviour
{
    const string HostName = "K-E36312A-80260";

    // Start is called before the first frame update
    void Start()
    {
        On();
    }

    public void On()
    {
        SetPower(true);
    }

    public void Off()
    {
        SetPower(false);
    }

    private void SetPower(bool on)
    {
        try
        {
            TelnetConnection tc = new TelnetConnection(HostName);
            tc.Open();

            if (tc.IsOpen)
            {
                // Set voltage and current on CH2
                tc.WriteLine("SOURCE:VOLTAGE 12, (@2)");
                tc.WriteLine("SOURCE:CURRENT 1, (@2)");

                tc.WriteLine("OUTPUT:STATE " + (on ? "1" : "0") + ", (@2)");

                tc.Dispose();
            }
            else
            {
                Console.WriteLine("Error opening " + HostName);
                return;
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
            return;
        }

    }
    // Update is called once per frame
    void Update()
    {
        
    }
}

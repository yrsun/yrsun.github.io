using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO.Ports;
using System.Text;




public class sp : MonoBehaviour
{
    #region GPS Variables
    public string DEVICE;
    public long TIME_STAMP;
    public string STATUS;
    public float LAT;
    public float LON;
    public float ALT;
    public int SAT;
    public int PREC;
    public float iniLAT;
    public float iniLON;
    public float iniALT;
    public bool firstGPSflag;

    private bool DEBUG = false;


    // private variables
    bool readDoneFlg = false;
    StringBuilder jsonBuffer = new StringBuilder();


    SerialPort streamCable = new SerialPort("COM4", 9600); //Set the port (com4) and the baud rate (9600, is standard on most devices)
    
    #endregion

    public float speed = 6.0F;
    public float jumpSpeed = 8.0F;
    public float gravity = 20.0F;
    private Vector3 moveDirection = Vector3.zero;

    public bool ccflag;

    // Use this for initialization
    void Start()
    {
        Debug.Log("Start  ");
        firstGPSflag = true;
        ccflag = true;
        //streamCable.DataReceived += new SerialDataReceivedEventHandler(StreamCable_DataReceived);
        streamCable.ReadTimeout = 100;
        streamCable.Open(); //Open the Serial Stream.
    }

    private void StreamCable_DataReceived(object sender, SerialDataReceivedEventArgs e)
    {
        if (readDoneFlg == true)
        {
            // wait for main program done with the current buffer
            return;
        }
        SerialPort sp = (SerialPort)sender;
        char indata = (char)sp.ReadChar();
        if (indata == '\n')
        {
            readDoneFlg = true;
        }else
        {
            if (DEBUG)
            {
                Debug.Log("read: " + indata);
            }
            
            jsonBuffer.Append(indata);
        }

    }

    // Update is called once per frame
    void Update()
    {
        if (!ccflag)
        {
            if (readGPS())
            {
                move();
            }
            if (Input.GetKeyDown(KeyCode.B))
            {
                ccflag = false;
                LAT = 0;
                LON = 0;
                iniLAT = 0;
                iniLON = 0;
            }
        }
        else
        {
            ccmove();
            if (Input.GetKeyDown(KeyCode.B))
            {
                ccflag = true;
            }
        }
        
        

    }

    bool readGPS()
    {
        
        if (!streamCable.IsOpen)
        {
            Debug.Log("Serial port it not open!");
        }
        char indata;
        while (true)
        {
            try
            {
                indata = (char)streamCable.ReadChar();
                if (indata == '\n')
                {
                    readDoneFlg = true;
                    if (DEBUG)
                    {
                        Debug.Log("read: " + jsonBuffer);
                    }
                   break;
                }
                else
                {
                    
                    jsonBuffer.Append(indata);
                }
            }
            catch (System.Exception)
            {

            }
        }
        
        
        

        if (readDoneFlg)
        {

            try
            {
                JsonUtility.FromJsonOverwrite(jsonBuffer.ToString(), this);
                jsonBuffer.Remove(0, jsonBuffer.Length);        // clear the buffer for the next reading
                readDoneFlg = false;
                if (DEBUG)
                {
                    Debug.Log("device = " + this.DEVICE);
                    Debug.Log("time_stamp = " + this.TIME_STAMP);
                    Debug.Log("status = " + this.STATUS);
                    Debug.Log("lat = " + this.LAT);
                    Debug.Log("lon = " + this.LON);
                    Debug.Log("alt = " + this.ALT);
                    Debug.Log("sat = " + this.SAT);
                    Debug.Log("prec = " + this.PREC);
                }
                    return true;
            }
            catch (System.Exception)
            {
                Debug.Log("fail to Json.");
                jsonBuffer.Remove(0, jsonBuffer.Length);
                readDoneFlg = false;
                throw;
            }
        }
        else
        {
            Debug.Log("No  ");
            return false;
        }
    }

    void ccmove()
    {
        CharacterController controller = GetComponent<CharacterController>();
        if (controller.isGrounded)
        {
            moveDirection = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
            moveDirection = transform.TransformDirection(moveDirection);
            moveDirection *= speed;
            if (Input.GetButton("Jump"))
                moveDirection.y = jumpSpeed;

        }
        moveDirection.y -= gravity * Time.deltaTime;
        controller.Move(moveDirection * Time.deltaTime);
    }

    void move()
    {
        if (firstGPSflag == true && LON != 0 && LAT != 0 && ALT != 0)
        {
            iniLON = LON;
            iniLAT = LAT;
            iniALT = ALT;
            firstGPSflag = false;
        }
        else if (LON != 0 && LAT != 0)
        {
            float la = (float)((LAT - 53.178469) / 0.00001 * 0.12179047095976932582726898256213);
            float lo = (float)((LON - 53.178469) / 0.00001 * 0.12179047095976932582726898256213);
            float ila = (float)((iniLAT - 53.178469) / 0.00001 * 0.12179047095976932582726898256213);
            float ilo = (float)((iniLON - 53.178469) / 0.00001 * 0.12179047095976932582726898256213);
            //transform.position = Quaternion.AngleAxis(LON-iniLON, -Vector3.up) * Quaternion.AngleAxis(LAT-iniLAT, -Vector3.right) * new Vector3(0, 0, 1);
            transform.position = new Vector3((lo-ilo), 0.5f, (la-ila));
        }
        
    }
}



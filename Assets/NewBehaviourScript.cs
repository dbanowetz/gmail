using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngineInternal;
using UnityEngine.UI;
using UnityEngine.Events;
using System;

public class NewBehaviourScript : MonoBehaviour {
    // Use this for initialization

    //List to hold all of the notifications
    private ArrayList notificationArray;
    private ArrayList shownArray;
    //Value to hold index of the current group of notifications being shown to the user
    private int currentShownIndexGroup;
    //Data member to hold the message from the input field
    private string storedMessage;
    private bool filteringToggle;
    [SerializeField]
    private bool sortMessage;
    [SerializeField]
    string filter;
    //Flag to determine whether the messages are sorted in ascending or descending order
    [SerializeField]
    private bool sortApplicationAsc;
    //Max number of notifications to store
    [SerializeField]
    private int notificationNumber;
   // Datamember to hold a reference to the input field in the scene
    InputField inputText;
    Button filterButton;
    //Font for the notifications
    string font;
    [SerializeField]
    bool timestampedNotfications;
    [SerializeField]
    GUIStyle style;

    #region Predicates

    //Comparer class that is used to sort an arraylist of strings in ascending order.
    public class notificationMessageAscendingComparer : IComparer
    {
        public int Compare(object x, object y)
        {
            //Testing to see if the objects being passed in are notification structs.
            if (!(x is Notification) || !(y is Notification))
            {
                return 0;
            }
            Notification a = (Notification)x;
            Notification b = (Notification)y;
            if(string.Compare(a.getApplicationString(), b.getApplicationString()) == 0)
            {
                return string.Compare(a.getMessageString(), b.getMessageString());

            }else
            {
                return string.Compare(a.getApplicationString(), b.getApplicationString());
            }

        }
    }

    //Comparer class that is used to sort an arraylist of strings in descending order.
    public class notificationMessageDescendingComparer : IComparer
    {
        public int Compare(object x, object y)
        {
            //Testing to see if the objects being passed in are notification structs.
            if (!(x is Notification) || !(y is Notification))
            {
                return 0;
            }
            Notification a = (Notification)x;
            Notification b = (Notification)y;
            if (string.Compare(a.getApplicationString(), b.getApplicationString()) == 0)
            {
                return string.Compare(b.getMessageString(), a.getMessageString());

            }
            else
            {
                return string.Compare(b.getApplicationString(), a.getApplicationString());
            }
           
        }
    }
    public class notifcationApplicationAscendingComparer : IComparer
    {
        public int Compare(object x, object y)
        {
            //Testing to see if the objects being passed in are notification structs.
            if (!(x is Notification) || !(y is Notification))
            {
                return 0;
            }
            Notification a = (Notification)x;
            Notification b = (Notification)y;

            return string.Compare(a.getApplicationString(), b.getApplicationString());
        }
    }
    public class notifcationApplicationDescendingComparer : IComparer
    {
        public int Compare(object x, object y)
        {
            //Testing to see if the objects being passed in are notification structs.
            if (!(x is Notification) || !(y is Notification))
            {
                return 0;
            }
            Notification a = (Notification)x;
            Notification b = (Notification)y;

            return string.Compare(b.getApplicationString(), a.getApplicationString());
        }
    }
    #endregion
    //structure for the notifications. Contains a string to hold the name of the application and a string to hold the message from the application that should be displayed.

    protected struct Notification
    {
        string application;
        string message;
        public Notification(string _application = null, string _message = null)
        {
            application = _application;
            message = _message;
        }

        //Getters and setters for the application and message strings.
        public string getApplicationString() { return application; }
        public void setApplicationString(string _application) { application = _application; }
        public string getMessageString() { return message; }
        public void setMessageString(string _message) { message = _message; }
    };
    
    //Function that is executed when a onEndEdit event is sent from the Input field.
    //It adds a new notification based on the text inside the input field.
    void lockInput(string args0)
    {
        if (inputText.text.Length > 0)
        {
            storedMessage = inputText.text;
            if (timestampedNotfications)
            {
                AddNewNotificationTimeStamp("InputField", storedMessage);
            }
            else
            {
                AddNewNotification("InputField", storedMessage);
            }
            inputText.text = "";
        }
    }
    //Ensures that there is only one instance of this script.
    public static NewBehaviourScript _instance = null;
    private void Awake()
    {
     
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(this.gameObject);
    }

  
    //The event listener is bound to the lockInput onEndEdit event.
    void Start () {
        

        notificationArray = new ArrayList();
        shownArray = new ArrayList();
        //inputText = gameObject.GetComponent<InputField>();
        //var se = new InputField.SubmitEvent();
        //se.AddListener(lockInput);
        //inputText.onEndEdit = se;
        style = new GUIStyle();
        filterButton = GameObject.FindObjectOfType<Button>();
        var see = new Button.ButtonClickedEvent();
        see.AddListener(startFiltering);
        filterButton.onClick = see;

        filteringToggle = false;
    }

    private void startFiltering()
    {
        shownArray.Clear();
        if (!filteringToggle)
        {
            
            for (int i = 0; i < notificationArray.Count; i++)
            {
                Notification temp = (Notification)notificationArray[i];
                if (!filter.Equals(temp.getApplicationString(), StringComparison.OrdinalIgnoreCase))
                {
                    shownArray.Add(temp);
                }
            }
            filteringToggle = !filteringToggle;
        }else
        {
            shownArray = (ArrayList)notificationArray.Clone();
            filteringToggle = !filteringToggle;

        }
        if (sortApplicationAsc)
        {
            shownArray.Sort(new notifcationApplicationAscendingComparer());

            if (sortMessage)
            {
                shownArray.Sort(new notificationMessageAscendingComparer());
            }
           
        }
        else
        {
            shownArray.Sort(new notifcationApplicationDescendingComparer());
            if (sortMessage)
            {
                shownArray.Sort(new notificationMessageDescendingComparer());
            }
           
        }

    }

    void FixedUpdate()
    {

        //Implement a solution for switching between the shown notifications.
        
            if (Input.GetKeyDown(KeyCode.P))
            {
                int diff = notificationArray.Count - currentShownIndexGroup;
                if (diff>10)
                {
                    if (currentShownIndexGroup != 100)
                    {

                        currentShownIndexGroup += 10;
                    }
                }
            }
            if (Input.GetKeyDown(KeyCode.O))
            {
                if (currentShownIndexGroup != 0)
                {
                    currentShownIndexGroup -= 10;
                }
            }
        
    }
    //This function adds a new notification and sorts the arraylist based on the sorting flag.
    public void AddNewNotification(string _application, string _message )
    {

        if (notificationArray.Count < notificationNumber)
        {
            Notification newMessage = new Notification();
            newMessage.setApplicationString(_application);
            newMessage.setMessageString(_message);
            notificationArray.Add(newMessage);
            Notification otherMessage = newMessage;
            shownArray.Add(otherMessage);
        }
        if (sortApplicationAsc)
        {
            shownArray.Sort(new notifcationApplicationAscendingComparer());

            if (sortMessage)
            {
                shownArray.Sort(new notificationMessageAscendingComparer());
            }

        }
        else
        {
            shownArray.Sort(new notifcationApplicationDescendingComparer());
            if (sortMessage)
            {
                shownArray.Sort(new notificationMessageDescendingComparer());
            }

        }
    }
    //This function adds a new notification with a timestamp and sorts the arraylist based on the sorting flag.
    public void AddNewNotificationTimeStamp(string _application, string _message)
    {

        if (notificationArray.Count < notificationNumber)
        {
            Notification newMessage = new Notification();
            string timestamped = _application + " " + System.DateTime.Now;
            if (string.Compare(_message, "Time") == 0)
            {
                newMessage.setApplicationString(timestamped);
                newMessage.setMessageString(_message);
                notificationArray.Add(newMessage);
                Notification otherMessage = newMessage;
                shownArray.Add(otherMessage);
            }else {
                newMessage.setApplicationString("Filter");
                newMessage.setMessageString(_message);
                notificationArray.Add(newMessage);
                Notification otherMessage = newMessage;
                shownArray.Add(otherMessage);
            }
            
        }

        if (sortApplicationAsc)
        {
            shownArray.Sort(new notifcationApplicationAscendingComparer());

            if (sortMessage)
            {
                shownArray.Sort(new notificationMessageAscendingComparer());
            }
            else
            {
                shownArray.Sort(new notificationMessageDescendingComparer());
            }

        }
        else
        {
            shownArray.Sort(new notifcationApplicationDescendingComparer());
            if (sortMessage)
            {
                shownArray.Sort(new notificationMessageDescendingComparer());
            }
            else
            {

            }

        }
    }
    //This function removes a notification from the array list
    public void RemoveNotification(int index)
    {
        if(index < notificationArray.Count&&index>=0)
        {
            notificationArray.RemoveAt(index);
        }
      
    }    
    void OnGUI()
    {

        
        for (int i = currentShownIndexGroup; i < currentShownIndexGroup + 10; i++)
            {
                if (i < shownArray.Count)
                {
                    Notification toDisplay = (Notification)shownArray[i];
                    string pulledTogether = toDisplay.getApplicationString() + ": " + toDisplay.getMessageString();
                    GUI.Label(new Rect(0, (i % 10) * 10, 300, 20), pulledTogether, style);
               
                }
            }

        }
    
        
}


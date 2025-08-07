public static class TRSWEBGlobal
{
    static string _lasturl;
    static string _url1;
    static string _url2;
    static bool _url1Avail;
    static bool _url2Avail;
    static string _URL1UnavailableStart;
    static string _URL2UnavailableStart;
    static int _url1UnavailCount;
    static int _url2UnavailCount;

    /// <summary>
    /// Get or set the static important data.
    /// </summary>
    public static string LastURL
    {
        get
        {
            return _lasturl;
        }
        set
        {
            _lasturl = value;
        }
    }

    public static int url1UnavailCount
    {
        get
        {
            return _url1UnavailCount;
        }
        set
        {
            _url1UnavailCount = value;
        }
    }

    public static int url2UnavailCount
    {
        get
        {
            return _url2UnavailCount;
        }
        set
        {
            _url2UnavailCount = value;
        }
    }

    public static string url1
    {
        get
        {
            return _url1;
        }
        set
        {
            _url1 = value;
        }
    }

    public static string url2
    {
        get
        {
            return _url2;
        }
        set
        {
            _url2 = value;
        }
    }

    public static bool url2Avail
    {
        get
        {
            return _url2Avail;
        }
        set
        {
            _url2Avail = value;
        }
    }

    public static bool url1Avail
    {
        get
        {
            return _url1Avail;
        }
        set
        {
            _url1Avail = value;
        }
    }


    public static string URL1UnavailableStart
    {
        get
        {
            return _URL1UnavailableStart;
        }
        set
        {
            _URL1UnavailableStart = value;
        }
    }

    public static string URL2UnavailableStart
    {
        get
        {
            return _URL2UnavailableStart;
        }
        set
        {
            _URL2UnavailableStart = value;
        }
    }

    public static string ResetURLAvailable
    {
        set
        {
            int resetMins = 5;

            if (TRS.IT.TrsAppSettings.AppSettings.GetValue("ResetURLMin") != null)
            {
                resetMins = Convert.ToInt32(TRS.IT.TrsAppSettings.AppSettings.GetValue("ResetURLMin"));
            }

            DateTime d = Convert.ToDateTime(URL1UnavailableStart).AddMinutes(resetMins);

            if (DateTime.Now > d)
            {
                url1Avail = true;
                URL1UnavailableStart = null;
                url1UnavailCount = 0;
            }

            d = Convert.ToDateTime(URL2UnavailableStart).AddMinutes(resetMins);
            if (DateTime.Now > d)
            {
                url2Avail = true;
                URL2UnavailableStart = null;
                url2UnavailCount = 0;
            }
        }
    }



}
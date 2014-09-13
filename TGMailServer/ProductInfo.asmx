  <%@ WebService language="C#" class="ProductInfo" %>

  using System;
  using System.Web.Services;
  using System.Xml.Serialization;

  // Portions adapted from "Web Service Versioning and Deprecation" by Jeff Kenyon
  [WebService(Namespace="http://www.vinfo.com/Updates/TGMailServer/")]

  public class ProductInfo : WebService
  {
	// Version Information
	private int VER_MAJOR = 0;
	private int VER_MINOR = 7;
	private int VER_PATCH = 7;
	private int VER_BUILD = 0;
	
	// Sponsor Site Information (Note trailing slashes)
	string[] Sponsors = new string[] 
	{
		"http://www.comla.com/",
		"http://www.funla.com/",
		"http://www.naecp.com/",
		"http://www.vinfo.com/",
		"http://www.voiceinformation.com/"
	};
	
	// Properties
	[WebMethod]
	public bool checkUpdate(int major, int minor, int patch, int build) 
	{
		if (getMajor() != major) return (getMajor() > major);
		if (getMinor() != minor) return (getMinor() > minor);
		if (getPatch() != patch) return (getPatch() > patch);
		if (getBuild() != build) return (getBuild() > build);
		return (false);
	}

	[WebMethod]
	public string getVersion() 
	{
		return (getMajor() + "." + getMinor() + "." + getPatch() + "." + getBuild());
	}

	[WebMethod]
	public int getMajor() 
	{
		return (VER_MAJOR);
	}

	[WebMethod]
	public int getMinor() 
	{
		return (VER_MINOR);
	}

	[WebMethod]
	public int getPatch() 
	{
		return (VER_PATCH);
	}

	[WebMethod]
	public int getBuild() 
	{
		return (VER_BUILD);
	}


	[WebMethod]
	public string[] getSponsors() 
	{
		return (Sponsors);
	}

}
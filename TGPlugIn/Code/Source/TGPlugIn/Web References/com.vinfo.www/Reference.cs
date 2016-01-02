﻿//------------------------------------------------------------------------------
// <autogenerated>
//     This code was generated by a tool.
//     Runtime Version: 1.1.4322.2032
//
//     Changes to this file may cause incorrect behavior and will be lost if 
//     the code is regenerated.
// </autogenerated>
//------------------------------------------------------------------------------

// 
// This source code was auto-generated by Microsoft.VSDesigner, Version 1.1.4322.2032.
// 
namespace TGPlugIn.com.vinfo.www {
    using System.Diagnostics;
    using System.Xml.Serialization;
    using System;
    using System.Web.Services.Protocols;
    using System.ComponentModel;
    using System.Web.Services;
    
    
    /// <remarks/>
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Web.Services.WebServiceBindingAttribute(Name="ProductInfoSoap", Namespace="http://www.vinfo.com/Updates/TGPlugIn/")]
    public class ProductInfo : System.Web.Services.Protocols.SoapHttpClientProtocol {
        
        /// <remarks/>
        public ProductInfo() {
            this.Url = "http://www.vinfo.com/Updates/TGPlugIn/ProductInfo.asmx";
        }
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://www.vinfo.com/Updates/TGPlugIn/checkUpdate", RequestNamespace="http://www.vinfo.com/Updates/TGPlugIn/", ResponseNamespace="http://www.vinfo.com/Updates/TGPlugIn/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public bool checkUpdate(int major, int minor, int patch, int build) {
            object[] results = this.Invoke("checkUpdate", new object[] {
                        major,
                        minor,
                        patch,
                        build});
            return ((bool)(results[0]));
        }
        
        /// <remarks/>
        public System.IAsyncResult BegincheckUpdate(int major, int minor, int patch, int build, System.AsyncCallback callback, object asyncState) {
            return this.BeginInvoke("checkUpdate", new object[] {
                        major,
                        minor,
                        patch,
                        build}, callback, asyncState);
        }
        
        /// <remarks/>
        public bool EndcheckUpdate(System.IAsyncResult asyncResult) {
            object[] results = this.EndInvoke(asyncResult);
            return ((bool)(results[0]));
        }
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://www.vinfo.com/Updates/TGPlugIn/getVersion", RequestNamespace="http://www.vinfo.com/Updates/TGPlugIn/", ResponseNamespace="http://www.vinfo.com/Updates/TGPlugIn/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public string getVersion() {
            object[] results = this.Invoke("getVersion", new object[0]);
            return ((string)(results[0]));
        }
        
        /// <remarks/>
        public System.IAsyncResult BegingetVersion(System.AsyncCallback callback, object asyncState) {
            return this.BeginInvoke("getVersion", new object[0], callback, asyncState);
        }
        
        /// <remarks/>
        public string EndgetVersion(System.IAsyncResult asyncResult) {
            object[] results = this.EndInvoke(asyncResult);
            return ((string)(results[0]));
        }
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://www.vinfo.com/Updates/TGPlugIn/getMajor", RequestNamespace="http://www.vinfo.com/Updates/TGPlugIn/", ResponseNamespace="http://www.vinfo.com/Updates/TGPlugIn/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public int getMajor() {
            object[] results = this.Invoke("getMajor", new object[0]);
            return ((int)(results[0]));
        }
        
        /// <remarks/>
        public System.IAsyncResult BegingetMajor(System.AsyncCallback callback, object asyncState) {
            return this.BeginInvoke("getMajor", new object[0], callback, asyncState);
        }
        
        /// <remarks/>
        public int EndgetMajor(System.IAsyncResult asyncResult) {
            object[] results = this.EndInvoke(asyncResult);
            return ((int)(results[0]));
        }
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://www.vinfo.com/Updates/TGPlugIn/getMinor", RequestNamespace="http://www.vinfo.com/Updates/TGPlugIn/", ResponseNamespace="http://www.vinfo.com/Updates/TGPlugIn/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public int getMinor() {
            object[] results = this.Invoke("getMinor", new object[0]);
            return ((int)(results[0]));
        }
        
        /// <remarks/>
        public System.IAsyncResult BegingetMinor(System.AsyncCallback callback, object asyncState) {
            return this.BeginInvoke("getMinor", new object[0], callback, asyncState);
        }
        
        /// <remarks/>
        public int EndgetMinor(System.IAsyncResult asyncResult) {
            object[] results = this.EndInvoke(asyncResult);
            return ((int)(results[0]));
        }
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://www.vinfo.com/Updates/TGPlugIn/getPatch", RequestNamespace="http://www.vinfo.com/Updates/TGPlugIn/", ResponseNamespace="http://www.vinfo.com/Updates/TGPlugIn/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public int getPatch() {
            object[] results = this.Invoke("getPatch", new object[0]);
            return ((int)(results[0]));
        }
        
        /// <remarks/>
        public System.IAsyncResult BegingetPatch(System.AsyncCallback callback, object asyncState) {
            return this.BeginInvoke("getPatch", new object[0], callback, asyncState);
        }
        
        /// <remarks/>
        public int EndgetPatch(System.IAsyncResult asyncResult) {
            object[] results = this.EndInvoke(asyncResult);
            return ((int)(results[0]));
        }
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://www.vinfo.com/Updates/TGPlugIn/getBuild", RequestNamespace="http://www.vinfo.com/Updates/TGPlugIn/", ResponseNamespace="http://www.vinfo.com/Updates/TGPlugIn/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public int getBuild() {
            object[] results = this.Invoke("getBuild", new object[0]);
            return ((int)(results[0]));
        }
        
        /// <remarks/>
        public System.IAsyncResult BegingetBuild(System.AsyncCallback callback, object asyncState) {
            return this.BeginInvoke("getBuild", new object[0], callback, asyncState);
        }
        
        /// <remarks/>
        public int EndgetBuild(System.IAsyncResult asyncResult) {
            object[] results = this.EndInvoke(asyncResult);
            return ((int)(results[0]));
        }
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://www.vinfo.com/Updates/TGPlugIn/getSponsors", RequestNamespace="http://www.vinfo.com/Updates/TGPlugIn/", ResponseNamespace="http://www.vinfo.com/Updates/TGPlugIn/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public string[] getSponsors() {
            object[] results = this.Invoke("getSponsors", new object[0]);
            return ((string[])(results[0]));
        }
        
        /// <remarks/>
        public System.IAsyncResult BegingetSponsors(System.AsyncCallback callback, object asyncState) {
            return this.BeginInvoke("getSponsors", new object[0], callback, asyncState);
        }
        
        /// <remarks/>
        public string[] EndgetSponsors(System.IAsyncResult asyncResult) {
            object[] results = this.EndInvoke(asyncResult);
            return ((string[])(results[0]));
        }
    }
}

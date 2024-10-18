using Orbit360.Common;
using Orbit360.DAL;
using Orbit360.Models;
using Orbit360.ViewModels;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Web;
using System.Web.Http;

namespace Orbit360.WebAPIs
{
    public class MainDepartmentController : ApiController
    {
        private Orbit360Context db = new Orbit360Context();
        string _SiteURL = ConfigurationManager.AppSettings["SiteURL"];
        string _ErrorMessage = ConfigurationManager.AppSettings["ResponseMessageNotAccess"];
        ApplicationHelper obj_ApplicationHelper = new ApplicationHelper();
        ResponseHelper obj_ResponseHelper = new ResponseHelper();

        //--Use -> [Restaurant-Admin-Web, Admin-Access-Restaurant-Admin-Web ,POS-App]
        //--Add New Main-Department-- 
        [Authorize(Roles = "SuperAdmin,Restaurant,RestaurantPOSApp,HeadOffice")]
        [Route("api/addupdate/maindepartment")]
        [HttpPost]
        public HttpResponseMessage InsertUpdateMainDepartmentData()
        {
            try
            {
                //--Get User Identity
                var identity = User.Identity as ClaimsIdentity;

                //--Check if user is authorized user or not
                if (identity != null)
                {
                    //--Create object of HttpRequest
                    var HttpRequest = HttpContext.Current.Request;
                    Int64 RestaurantLoginId_Param_Value = (string.IsNullOrEmpty(HttpRequest.Params["restaurantLoginId"]) ? 0 : Convert.ToInt64(HttpRequest.Params["restaurantLoginId"]));

                    //--Get Logged-In User's Login-Detail 
                    UserLogins_VM _UserLogin = obj_ApplicationHelper.Get_UserLoginDetail_By_UserLoginId(identity, RestaurantLoginId_Param_Value);

                    ResponseViewModel_WithId _resp = new ResponseViewModel_WithId();
                   
                    if (_UserLogin != null)
                    {
                        //--Validate Platform like (if POS/Kiosk device that should be Connected/Activated)
                        ResponseViewModel _resp_PlatformValidation = obj_ApplicationHelper.Validate_Platform_Handler(_UserLogin.UserTokenIdentity_Data);

                        //--Check if Platform is valid
                        if(_resp_PlatformValidation.ret == 1)
                        {
                            Int64 _RestaurantLoginId = _UserLogin.RestaurantLoginId;

                            //--Get all parameter's value of Form-Data (by Key-Name)
                            Int64 _Id = Convert.ToInt64(HttpRequest.Params["id"]);
                            string _MainDepartmentName = HttpRequest.Params["name"];
                            int _Mode = Convert.ToInt32(HttpRequest.Params["mode"]);

                            //--if restaurant-exist
                            if (_RestaurantLoginId > 0)
                            {
                                //--check if main-department is null or empty
                                if (string.IsNullOrEmpty(_MainDepartmentName))
                                {
                                    //--Create response
                                    var objResponse = new
                                    {
                                        status = -5,
                                        message = "Please provide main-department name!"
                                    };

                                    //sending response as OK
                                    return Request.CreateResponse(HttpStatusCode.OK, objResponse);
                                }
                                else
                                {
                                    //--Add/Update Main-Department Info
                                    SqlParameter[] queryParams = new SqlParameter[] {
                                    new SqlParameter("id", _Id),
                                    new SqlParameter("restaurantLoginId", _RestaurantLoginId),
                                    new SqlParameter("name", _MainDepartmentName),
                                    new SqlParameter("submittedByLoginId", _UserLogin.Id),
                                    new SqlParameter("mode", _Mode)
                                    };

                                    _resp = db.Database.SqlQuery<ResponseViewModel_WithId>("exec sp_InsertUpdateMainDepartment @id,@restaurantLoginId,@name,@submittedByLoginId,@mode", queryParams).FirstOrDefault();

                                    //--Create response
                                    var objResponse = new
                                    {
                                        status = _resp.ret,
                                        message = _resp.responseMessage,
                                        mainDepartmentId = _resp.Id
                                    };

                                    //sending response as OK
                                    return Request.CreateResponse(HttpStatusCode.OK, objResponse);
                                }
                            }
                            else
                            {
                                //--Create response
                                var objResponse = new
                                {
                                    status = 0,
                                    message = "Sorry, unable to access data, please login into the panel again!",
                                };

                                //sending response as OK
                                return Request.CreateResponse(HttpStatusCode.OK, objResponse);
                            }
                        }
                        else
                        {
                            //--Create response
                            var objResponse = obj_ResponseHelper.Get_PlatformValidation_Error_Response(_resp_PlatformValidation);

                            //sending response
                            return Request.CreateResponse(obj_ResponseHelper.Get_PlatformValidation_Error_ResponseStatus(), objResponse);
                        }
                    }
                    else
                    {
                        //--Create response as Un-Authorized
                        var objResponse = new { status = -101, message = "Authorization has been denied for this request!", data = "" };
                        //sending response as Un-Authorized
                        return Request.CreateResponse(HttpStatusCode.Unauthorized, objResponse);
                    }
                }
                else
                {
                    //--Create response as Un-Authorized
                    var objResponse = new { status = -101, message = "Authorization has been denied for this request!", data = "" };
                    //sending response as Un-Authorized
                    return Request.CreateResponse(HttpStatusCode.Unauthorized, objResponse);
                }
            }
            catch (Exception ex)
            {
                //--Create response as Error
                var objResponse = new { status = -100, message = "Internal Server Error!", data = "" };
                //sending response as error
                return Request.CreateResponse(HttpStatusCode.InternalServerError, objResponse);
            }
        }

        //--Use -> [Restaurant-Admin-Web, Admin-Access-Restaurant-Admin-Web ,POS-App]
        //--Get All Main-Departments-List by Specific Restaurant-- 
        [Authorize(Roles = "SuperAdmin,Restaurant,RestaurantPOSApp,HeadOffice")]
        [Route("api/maindepartment/list")]
        [HttpGet]
        public HttpResponseMessage GetAllMainDepartmentsListData(Int64 restaurantLoginId = 0)
        {
            string _NullData = null;

            try
            {
                //--Get User Identity
                var identity = User.Identity as ClaimsIdentity;

                //--Check if user is authorized user or not
                if (identity != null)
                {
                    //--Get Logged-In User's Login-Detail 
                    UserLogins_VM _UserLogin = obj_ApplicationHelper.Get_UserLoginDetail_By_UserLoginId(identity, restaurantLoginId);

                    if (_UserLogin != null)
                    {
                        //--Validate Platform like (if POS/Kiosk device that should be Connected/Activated)
                        ResponseViewModel _resp_PlatformValidation = obj_ApplicationHelper.Validate_Platform_Handler(_UserLogin.UserTokenIdentity_Data);

                        //--Check if Platform is valid
                        if(_resp_PlatformValidation.ret == 1)
                        {
                            Int64 _RestaurantLoginId = _UserLogin.RestaurantLoginId;

                            //--if restaurant-exist
                            if (_RestaurantLoginId > 0)
                            {
                                List<MainDepartmentViewModel> lstMainDepartment = new List<MainDepartmentViewModel>();

                                //--Get All Departments-List by Restaurant-Login-id
                                SqlParameter[] queryParams = new SqlParameter[] {
                                new SqlParameter("id", "0"),
                                new SqlParameter("restaurantLoginId", _RestaurantLoginId),
                                new SqlParameter("mode", "1")
                                };
                                lstMainDepartment = db.Database.SqlQuery<MainDepartmentViewModel>("exec sp_ManageMainDepartment @id,@restaurantLoginId,@mode", queryParams).ToList();

                                //--Create response
                                var objResponse = new
                                {
                                    status = 1,
                                    message = "Success",
                                    data = new
                                    { mainDepartments = lstMainDepartment }
                                };

                                //sending response as OK
                                return Request.CreateResponse(HttpStatusCode.OK, objResponse);
                            }
                            else
                            {
                                //--Create response
                                var objResponse = new
                                {
                                    status = 0,
                                    message = _ErrorMessage,
                                    data = _NullData
                                };

                                //sending response as OK
                                return Request.CreateResponse(HttpStatusCode.OK, objResponse);
                            }
                        }
                        else
                        {
                            //--Create response
                            var objResponse = obj_ResponseHelper.Get_PlatformValidation_Error_Response(_resp_PlatformValidation);

                            //sending response
                            return Request.CreateResponse(obj_ResponseHelper.Get_PlatformValidation_Error_ResponseStatus(), objResponse);
                        }
                    }
                    else
                    {
                        //--Create response as Un-Authorized
                        var objResponse = new { status = -101, message = "Authorization has been denied for this request!", data = _NullData };
                        //sending response as Un-Authorized
                        return Request.CreateResponse(HttpStatusCode.Unauthorized, objResponse);
                    }
                }
                else
                {
                    //--Create response as Un-Authorized
                    var objResponse = new { status = -101, message = "Authorization has been denied for this request!", data = _NullData };
                    //sending response as Un-Authorized
                    return Request.CreateResponse(HttpStatusCode.Unauthorized, objResponse);
                }
            }
            catch (Exception ex)
            {
                //--Create response as Error
                var objResponse = new { status = -100, message = "Internal Server Error!", data = _NullData };
                //sending response as error
                return Request.CreateResponse(HttpStatusCode.InternalServerError, objResponse);
            }
        }

        //--Use -> [Restaurant-Admin-Web, Admin-Access-Restaurant-Admin-Web ,POS-App]
        //--Get All Active Main-Departments-List by Specific Restaurant-- 
        [Authorize(Roles = "SuperAdmin,Restaurant,RestaurantPOSApp,HeadOffice")]
        [Route("api/active/maindepartment/list")]
        [HttpGet]
        public HttpResponseMessage GetAllActiveMainDepartmentsListData(Int64 restaurantLoginId = 0)
        {
            string _NullData = null;

            try
            {
                //--Get User Identity
                var identity = User.Identity as ClaimsIdentity;

                //--Check if user is authorized user or not
                if (identity != null)
                {
                    //--Get Logged-In User's Login-Detail 
                    UserLogins_VM _UserLogin = obj_ApplicationHelper.Get_UserLoginDetail_By_UserLoginId(identity, restaurantLoginId);

                    if (_UserLogin != null)
                    {
                        //--Validate Platform like (if POS/Kiosk device that should be Connected/Activated)
                        ResponseViewModel _resp_PlatformValidation = obj_ApplicationHelper.Validate_Platform_Handler(_UserLogin.UserTokenIdentity_Data);

                        //--Check if Platform is valid
                        if(_resp_PlatformValidation.ret == 1)
                        {
                            Int64 _RestaurantLoginId = _UserLogin.RestaurantLoginId;

                            //--if restaurant-exist
                            if (_RestaurantLoginId > 0)
                            {
                                List<MainDepartmentViewModel> lstMainDepartment = new List<MainDepartmentViewModel>();

                                //--Get All Active Main-Departments-List by Restaurant-Login-id
                                SqlParameter[] queryParams = new SqlParameter[] {
                                new SqlParameter("id", "0"),
                                new SqlParameter("restaurantLoginId", _RestaurantLoginId),
                                new SqlParameter("mode", "4")
                                };
                                lstMainDepartment = db.Database.SqlQuery<MainDepartmentViewModel>("exec sp_ManageMainDepartment @id,@restaurantLoginId,@mode", queryParams).ToList();

                                //--Create response
                                var objResponse = new
                                {
                                    status = 1,
                                    message = "Success",
                                    data = new
                                    { mainDepartments = lstMainDepartment }
                                };

                                //sending response as OK
                                return Request.CreateResponse(HttpStatusCode.OK, objResponse);
                            }
                            else
                            {
                                //--Create response
                                var objResponse = new
                                {
                                    status = 0,
                                    message = _ErrorMessage,
                                    data = _NullData
                                };

                                //sending response as OK
                                return Request.CreateResponse(HttpStatusCode.OK, objResponse);
                            }
                        }
                        else
                        {
                            //--Create response
                            var objResponse = obj_ResponseHelper.Get_PlatformValidation_Error_Response(_resp_PlatformValidation);

                            //sending response
                            return Request.CreateResponse(obj_ResponseHelper.Get_PlatformValidation_Error_ResponseStatus(), objResponse);
                        }
                    }
                    else
                    {
                        //--Create response as Un-Authorized
                        var objResponse = new { status = -101, message = "Authorization has been denied for this request!", data = _NullData };
                        //sending response as Un-Authorized
                        return Request.CreateResponse(HttpStatusCode.Unauthorized, objResponse);
                    }
                }
                else
                {
                    //--Create response as Un-Authorized
                    var objResponse = new { status = -101, message = "Authorization has been denied for this request!", data = _NullData };
                    //sending response as Un-Authorized
                    return Request.CreateResponse(HttpStatusCode.Unauthorized, objResponse);
                }
            }
            catch (Exception ex)
            {
                //--Create response as Error
                var objResponse = new { status = -100, message = "Internal Server Error!", data = _NullData };
                //sending response as error
                return Request.CreateResponse(HttpStatusCode.InternalServerError, objResponse);
            }
        }

        //--Use -> [Restaurant-Admin-Web, Admin-Access-Restaurant-Admin-Web ,POS-App]
        //--Get Single Main-Department-Detail by Id-- 
        [Authorize(Roles = "SuperAdmin,Restaurant,RestaurantPOSApp,HeadOffice")]
        [Route("api/single/maindepartment")]
        [HttpGet]
        public HttpResponseMessage GetSingleMainDepartmentDetail(Int64 mainDepartmentId)
        {
            string _NullData = null;

            try
            {
                //--Get User Identity
                var identity = User.Identity as ClaimsIdentity;

                //--Check if user is authorized user or not
                if (identity != null)
                {
                    //--Get Logged-In User's Login-Detail 
                    UserLogins_VM _UserLogin = obj_ApplicationHelper.Get_UserLoginDetail_By_UserLoginId(identity, 0);

                    if (_UserLogin != null)
                    {
                        //--Validate Platform like (if POS/Kiosk device that should be Connected/Activated)
                        ResponseViewModel _resp_PlatformValidation = obj_ApplicationHelper.Validate_Platform_Handler(_UserLogin.UserTokenIdentity_Data);

                        //--Check if Platform is valid
                        if(_resp_PlatformValidation.ret == 1)
                        {
                            MainDepartmentViewModel lstMainDepartment = new MainDepartmentViewModel();

                            //--Get Single Main-Department-Detail by Main-Department-id
                            SqlParameter[] queryParams = new SqlParameter[] {
                            new SqlParameter("id", mainDepartmentId),
                            new SqlParameter("restaurantLoginId", "0"),
                            new SqlParameter("mode", "2")
                            };
                            lstMainDepartment = db.Database.SqlQuery<MainDepartmentViewModel>("exec sp_ManageMainDepartment @id,@restaurantLoginId,@mode", queryParams).FirstOrDefault();

                            //--Create response
                            var objResponse = new
                            {
                                status = 1,
                                message = "Success",
                                data = new
                                { mainDepartment = lstMainDepartment }
                            };

                            //sending response as OK
                            return Request.CreateResponse(HttpStatusCode.OK, objResponse);                           
                        }
                        else
                        {
                            //--Create response
                            var objResponse = obj_ResponseHelper.Get_PlatformValidation_Error_Response(_resp_PlatformValidation);

                            //sending response
                            return Request.CreateResponse(obj_ResponseHelper.Get_PlatformValidation_Error_ResponseStatus(), objResponse);
                        }
                    }
                    else
                    {
                        //--Create response as Un-Authorized
                        var objResponse = new { status = -101, message = "Authorization has been denied for this request!", data = _NullData };
                        //sending response as Un-Authorized
                        return Request.CreateResponse(HttpStatusCode.Unauthorized, objResponse);
                    }
                }
                else
                {
                    //--Create response as Un-Authorized
                    var objResponse = new { status = -101, message = "Authorization has been denied for this request!", data = _NullData };
                    //sending response as Un-Authorized
                    return Request.CreateResponse(HttpStatusCode.Unauthorized, objResponse);
                }
            }
            catch (Exception ex)
            {
                //--Create response as Error
                var objResponse = new { status = -100, message = "Internal Server Error!", data = _NullData };
                //sending response as error
                return Request.CreateResponse(HttpStatusCode.InternalServerError, objResponse);
            }
        }

        //--Use -> [Restaurant-Admin-Web, Admin-Access-Restaurant-Admin-Web ,POS-App]
        //--Delete Main-Department by Id-- 
        [Authorize(Roles = "SuperAdmin,Restaurant,RestaurantPOSApp,HeadOffice")]
        [Route("api/delete/maindepartment")]
        [HttpGet]
        public HttpResponseMessage DeleteMainDepartmentDetail(Int64 mainDepartmentId, Int64 restaurantLoginId = 0)
        {
            string _NullData = null;

            try
            {
                //--Get User Identity
                var identity = User.Identity as ClaimsIdentity;

                //--Check if user is authorized user or not
                if (identity != null)
                {
                    //--Get Logged-In User's Login-Detail 
                    UserLogins_VM _UserLogin = obj_ApplicationHelper.Get_UserLoginDetail_By_UserLoginId(identity, restaurantLoginId);

                    if (_UserLogin != null)
                    {
                        //--Validate Platform like (if POS/Kiosk device that should be Connected/Activated)
                        ResponseViewModel _resp_PlatformValidation = obj_ApplicationHelper.Validate_Platform_Handler(_UserLogin.UserTokenIdentity_Data);

                        //--Check if Platform is valid
                        if(_resp_PlatformValidation.ret == 1)
                        {
                            Int64 _RestaurantLoginId = _UserLogin.RestaurantLoginId;

                            //--if restaurant - exist
                            if (_RestaurantLoginId > 0)
                            {
                                ResponseViewModel resp = new ResponseViewModel();

                                //--Get Single Main-Department-Detail by Main-Department-id
                                SqlParameter[] queryParams = new SqlParameter[] {
                                new SqlParameter("id", mainDepartmentId),
                                new SqlParameter("restaurantLoginId", _RestaurantLoginId),
                                new SqlParameter("mode", "3")
                                };
                                resp = db.Database.SqlQuery<ResponseViewModel>("exec sp_ManageMainDepartment @id,@restaurantLoginId,@mode", queryParams).FirstOrDefault();

                                //--Create response
                                var objResponse = new
                                {
                                    status = resp.ret,
                                    message = resp.responseMessage
                                };

                                //sending response as OK
                                return Request.CreateResponse(HttpStatusCode.OK, objResponse);
                            }
                            else
                            {
                                //--Create response
                                var objResponse = new
                                {
                                    status = 0,
                                    message = _ErrorMessage,
                                    data = _NullData
                                };

                                //sending response as OK
                                return Request.CreateResponse(HttpStatusCode.OK, objResponse);
                            }
                        }
                        else
                        {
                            //--Create response
                            var objResponse = obj_ResponseHelper.Get_PlatformValidation_Error_Response(_resp_PlatformValidation);

                            //sending response
                            return Request.CreateResponse(obj_ResponseHelper.Get_PlatformValidation_Error_ResponseStatus(), objResponse);
                        }
                    }
                    else
                    {
                        //--Create response as Un-Authorized
                        var objResponse = new { status = -101, message = "Authorization has been denied for this request!", data = _NullData };
                        //sending response as Un-Authorized
                        return Request.CreateResponse(HttpStatusCode.Unauthorized, objResponse);
                    }
                }
                else
                {
                    //--Create response as Un-Authorized
                    var objResponse = new { status = -101, message = "Authorization has been denied for this request!", data = _NullData };
                    //sending response as Un-Authorized
                    return Request.CreateResponse(HttpStatusCode.Unauthorized, objResponse);
                }
            }
            catch (Exception ex)
            {
                //--Create response as Error
                var objResponse = new { status = -100, message = "Internal Server Error!", data = _NullData };
                //sending response as error
                return Request.CreateResponse(HttpStatusCode.InternalServerError, objResponse);
            }
        }
    }
}

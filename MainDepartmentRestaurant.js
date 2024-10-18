var UserToken_Global = "";
var RestaurantLoginId_Global = 0;
var isSuccess_MainDepartment_Global = -1;
var MainDepartmentId_Global = 0;
var isEnabled_DeleteOption_Global = 0;

$(document).ready(function () {
    StartLoading();

    //--Check Restaurant-Login-Token
    $.get("/Restaurant/GetRestaurantCookieDetail", null, function (dataRestaurantToken) {
        if (dataRestaurantToken != "" && dataRestaurantToken != null) {

            UserToken_Global = dataRestaurantToken;
            RestaurantLoginId_Global = 0;

            //--Get All Main-Departments List of Restaurant
            GetAllMainDepartmentsList();
        }
        else {
            //--Check Restaurant-Panel Access Detail
            $.get("/SuperAdmin/GetRPAccessCookieDetail", null, function (dataResp) {

                if (dataResp.status == 1 && dataResp.token != "" && dataResp.token != null && dataResp.restaurantLoginId > 0) {

                    UserToken_Global = dataResp.token;
                    RestaurantLoginId_Global = dataResp.restaurantLoginId;

                    //--Get All Main-Departments List of Restaurant
                    GetAllMainDepartmentsList();
                }
                else if (dataResp.status == -1) {
                    window.location = "/SuperAdmin/Login";
                }
                else {
                    //--Check HeadOffice-Panel Access Detail
                    $.get("/HeadOffice/GetRPAccessCookieDetailFromHeadOffice", null, function (dataResp) {

                        if (dataResp.status == 1 && dataResp.token != "" && dataResp.token != null && dataResp.restaurantLoginId > 0) {

                            UserToken_Global = dataResp.token;
                            RestaurantLoginId_Global = dataResp.restaurantLoginId;

                            //--Get All Main-Departments List of Restaurant
                            GetAllMainDepartmentsList();
                        }
                        else if (dataResp.status == -1) {
                            window.location = "/HeadOffice/Login";
                        }
                        else {
                            window.location = "/Restaurant/Login";
                        }
                    });
                }
            });
        }
    });
});

function OpenCreateMainDepartmentPopup() {
    $("#txtMainDepartmentName_ManageMainDepartment").val('');
    $(".errorsClass2").html('');

    //--Set Title
    $("#heading_Title_MainDepartmentModal").html('Add Main Department');

    $("#btnSubmit_MainDepartment").show();
    $("#btnUpdate_MainDepartment").hide();

    //--Set main-department id in the global variable
    MainDepartmentId_Global = 0;

    //--Open Modal
    $("#btn_CreateMainDepartment_Modal").click();
}

function GetAllMainDepartmentsList() {

    $(".mainDepartmentsListClass").remove();

    $.ajax({
        type: "GET",
        url: "/api/maindepartment/list?restaurantLoginId=" + RestaurantLoginId_Global,
        headers: {
            "Authorization": "Bearer " + UserToken_Global,
            "Content-Type": "application/json"
        },
        contentType: 'application/json',
        success: function (dataMainDepartments) {

            //console.log(dataMainDepartments);

            if (dataMainDepartments.status == 1) {

                if (dataMainDepartments.data.mainDepartments.length > 0) {

                    var res_mainDepartments = '';
                    var isDisplay_DeleteIcon = 'display:none;';
                    var _name_MainDepartment = '';

                    //--Check if Delete Option is Enabled
                    if (isEnabled_DeleteOption_Global == 1) {

                        //--show delete-icons
                        isDisplay_DeleteIcon = '';
                    }

                    for (var i = 0; i < dataMainDepartments.data.mainDepartments.length; i++) {

                        if (dataMainDepartments.data.mainDepartments[i].Name.length > 30) {

                            _name_MainDepartment = dataMainDepartments.data.mainDepartments[i].Name.slice(0, 30) + '...';
                        }
                        else {
                            _name_MainDepartment = dataMainDepartments.data.mainDepartments[i].Name;
                        }

                        res_mainDepartments += '<div class="col-sm-2 mainDepartmentsListClass">' +
                            '<div class="wrap_chekox-remove">' +
                            '<span><a href="javascript:;" title="' + dataMainDepartments.data.mainDepartments[i].Name +'" onclick="EditMainDepartment(' + dataMainDepartments.data.mainDepartments[i].Id + ');">' + _name_MainDepartment +'</a></span>' +
                            '<span class="removecions removeIconMainDepartmentClass" style="' + isDisplay_DeleteIcon + '" onclick="ConfirmDeleteMainDepartment(' + dataMainDepartments.data.mainDepartments[i].Id +');"><span class="material-symbols-outlined">remove</span></span>' +
                            '</div >' +
                            '</div > ';
                    }

                    $("#dv_MainDeparmentsList_Section").prepend(res_mainDepartments);
                }

                StopLoading();
            }
            else {
                $("#lblResponseMessage_MainDepartment_Modal").html(dataMainDepartments.message);
                $("#iconError_MainDepartmentModal").show();
                $("#iconSuccess_MainDepartmentModal").hide();
                //--Open response-message popup
                $("#btn_ResponseMainDepartment_Modal").click();

                StopLoading();
            }
        },
        error: function (result) {
            StopLoading();

            if (result["status"] == 401) {
                $.iaoAlert({
                    msg: 'Unauthorized! Invalid Token!',
                    type: "error",
                    mode: "dark",
                });
            }
            else {
                $.iaoAlert({
                    msg: 'There is some technical error, please try again!',
                    type: "error",
                    mode: "dark",
                });
            }
        }
    });
}

function CloseCreateMainDepartment() {
    $("#txtMainDepartmentName_ManageMainDepartment").val('');
    $(".errorsClass2").html('');

    $("#btnSubmit_MainDepartment").show();
    $("#btnUpdate_MainDepartment").hide();

    //--Set Title
    $("#heading_Title_MainDepartmentModal").html('Add Main Department');

    //--Set main-department id in the global variable
    MainDepartmentId_Global = 0;

    //--Open Modal
    $("#btnCancel_CreateMainDepartment_Modal").click();
}

function CreateMainDepartment(_mode) {
    var _mainDepartment_MMD = $("#txtMainDepartmentName_ManageMainDepartment").val();

    //--Validate fields
    if (ValidateFields_ManageMainDepartment()) {

        StartLoading();

        var data = new FormData();

        data.append("id", MainDepartmentId_Global);
        data.append("restaurantLoginId", RestaurantLoginId_Global);
        data.append("name", _mainDepartment_MMD.toString().trim());
        data.append("mode", _mode);

        $.ajax({
            url: '/api/addupdate/maindepartment',
            headers: {
                "Authorization": "Bearer " + UserToken_Global
            },
            data: data,
            processData: false,
            mimeType: 'multipart/form-data',
            contentType: false,
            //contentType: 'application/json',
            type: 'POST',
            success: function (dataResponse) {

                //--Parse into Json of response-json-string
                dataResponse = JSON.parse(dataResponse);

                //--If successfully added/updated
                if (dataResponse.status == 1 || dataResponse.status == 2) {

                    //---------------------------Set Default data---------------------
                    $("#txtMainDepartmentName_ManageMainDepartment").val('');
                    $(".errorsClass2").html('');

                    $("#btnSubmit_MainDepartment").show();
                    $("#btnUpdate_MainDepartment").hide();

                    //--Set Title
                    $("#heading_Title_MainDepartmentModal").html('Add Main Department');

                    //--Set main-department id in the global variable
                    MainDepartmentId_Global = 0;
                    //-----------------------------------------------------------------

                    SuccessMsg(dataResponse.message);
                    //--Close Create-Main-Department popup
                    $("#btnCancel_CreateMainDepartment_Modal").click();

                    //--Get All Main-Departments List of Restaurant
                    GetAllMainDepartmentsList();
                }
                else {

                    //--------------Show Response Message Popup------------------
                    $("#iconError_MainDepartmentModal").show();
                    $("#iconSuccess_MainDepartmentModal").hide();

                    $("#lblResponseMessage_MainDepartment_Modal").html(dataResponse.message);

                    //--Set Failed
                    isSuccess_MainDepartment_Global = 0;

                    //--Close Create-Main-Department popup
                    $("#btnCancel_CreateMainDepartment_Modal").click();

                    //--Show Response-Message popup
                    $("#btn_ResponseMainDepartment_Modal").click();
                    //------------------------------------------------------------

                    StopLoading();
                }
            },
            error: function (result) {
                StopLoading();

                if (result["status"] == 401) {
                    $.iaoAlert({
                        msg: 'Unauthorized! Invalid Token!',
                        type: "error",
                        mode: "dark",
                    });
                }
                else {
                    $.iaoAlert({
                        msg: 'There is some technical error, please try again!',
                        type: "error",
                        mode: "dark",
                    });
                }
            }
        });
    }
}

function ValidateFields_ManageMainDepartment() {
   
    var _mainDepartment_MMD = $("#txtMainDepartmentName_ManageMainDepartment").val();

    var _is_valid = true;
    $(".errorsClass2").html('');

    if (_mainDepartment_MMD == "" || _mainDepartment_MMD.replace(/\s/g, "") == "") {
        _is_valid = false;
        $("#mainDepartmentName_error_ManageMainDepartment").html('Please enter main-department!');
    }
   
    return _is_valid;
}

function CloseResponseMessageModal_MainDepartment() {

    //--Close response Modal
    $("#btnClose_ResponseMessage_Modal_MainDepartment").click();

    if (isSuccess_MainDepartment_Global == 0) {

        //--Show Main-Department form popup
        $("#btn_CreateMainDepartment_Modal").click();
    }

    isSuccess_MainDepartment_Global = -1;
}

function EditMainDepartment(_mainDepartmentId) {

    StartLoading();

    $("#txtMainDepartmentName_ManageMainDepartment").val('');
    $(".errorsClass2").html('');

    //--Set main-department id in the global variable
    MainDepartmentId_Global = _mainDepartmentId;

    $.ajax({
        type: "GET",
        url: "/api/single/maindepartment?mainDepartmentId=" + _mainDepartmentId,
        headers: {
            "Authorization": "Bearer " + UserToken_Global,
            "Content-Type": "application/json"
        },
        contentType: 'application/json',
        success: function (dataMainDepartments) {

            if (dataMainDepartments.data.mainDepartment) {

                $("#txtMainDepartmentName_ManageMainDepartment").val(dataMainDepartments.data.mainDepartment.Name);

                $("#btnSubmit_MainDepartment").hide();
                $("#btnUpdate_MainDepartment").show();

                //--Set Title
                $("#heading_Title_MainDepartmentModal").html('Update Main Department');

                //--Open Modal
                $("#btn_CreateMainDepartment_Modal").click();
            }

            StopLoading();
        },
        error: function (result) {
            StopLoading();

            if (result["status"] == 401) {
                $.iaoAlert({
                    msg: 'Unauthorized! Invalid Token!',
                    type: "error",
                    mode: "dark",
                });
            }
            else {
                $.iaoAlert({
                    msg: 'There is some technical error, please try again!',
                    type: "error",
                    mode: "dark",
                });
            }
        }
    });
}

function EnableDeleteMainDepartmentOption() {

    if ($('#chkDeleteMainDepartment').is(':checked')) {
        // checked

        //--set in global variable
        isEnabled_DeleteOption_Global = 1;

        //--show delete-icon
        $(".removeIconMainDepartmentClass").show();
    } else {
        // unchecked
        //--set in global variable
        isEnabled_DeleteOption_Global = 0;

        //--hide delete-icon
        $(".removeIconMainDepartmentClass").hide();
    }
}

function ConfirmDeleteMainDepartment(_mainDepartmentId) {
    swal({
        title: "Delete Main-Department",
        text: "Are you sure to delete this Main-Department?",
        type: "warning",
        showCancelButton: true,
        confirmButtonColor: '#DD6B55',
        confirmButtonText: 'Yes',
        cancelButtonText: "No"
    }, function (isConfirm) {
        if (!isConfirm) return;
        DeleteMainDepartment(_mainDepartmentId);
    });
}

function DeleteMainDepartment(_mainDepartmentId) {

    StartLoading();

    $.ajax({
        type: "GET",
        url: "/api/delete/maindepartment?mainDepartmentId=" + _mainDepartmentId + "&restaurantLoginId=" + RestaurantLoginId_Global,
        headers: {
            "Authorization": "Bearer " + UserToken_Global,
            "Content-Type": "application/json"
        },
        contentType: 'application/json',
        success: function (dataResponse) {

            //--if successfully deleted
            if (dataResponse.status == 1) {

                SuccessMsg(dataResponse.message);

                //--Get All Main-Departments List of Restaurant
                GetAllMainDepartmentsList();
            }
            else {
                $("#lblResponseMessage_MainDepartment_Modal").html(dataResponse.message);
                //--Set nothing
                isSuccess_MainDepartment_Global = -1;
                $("#iconError_MainDepartmentModal").show();
                $("#iconSuccess_MainDepartmentModal").hide();

                //--Open response-message popup
                $("#btn_ResponseMainDepartment_Modal").click();

                StopLoading();
            }
        },
        error: function (result) {
            StopLoading();

            if (result["status"] == 401) {
                $.iaoAlert({
                    msg: 'Unauthorized! Invalid Token!',
                    type: "error",
                    mode: "dark",
                });
            }
            else {
                $.iaoAlert({
                    msg: 'There is some technical error, please try again!',
                    type: "error",
                    mode: "dark",
                });
            }
        }
    });
}
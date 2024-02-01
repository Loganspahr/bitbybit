$(document).ready(function () {
    $('#loginForm').submit(function (event) {
        event.preventDefault();
        loginHandler();
    });
});

function loginHandler() {
    var webMethod = "ProjectServices.asmx/LogOn";
    var parameters = JSON.stringify({
        uid: $('#username').val(),
        pass: $('#password').val()
    });

    $.ajax({
        type: "POST",
        url: webMethod,
        data: parameters,
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (msg) {
            var success = msg.d;
            if (success) {
                // Redirect to a new page or change the view
                window.location.replace('dashboard.html'); // Replace with your actual dashboard page
                alert('Login successful!');
            } else {
                // Implement lockout logic or error handling
                alert('Login failed: Invalid username or password.');
            }
        },
        error: function (e) {
            // Handle connection errors
            alert("There was an issue with the login request.");
        }
    });
}

function TestButtonHandler() {
    var webMethod = "ProjectServices.asmx/TestConnection";
    var parameters = "{}";

    $.ajax({
        type: "POST",
        url: webMethod,
        data: parameters,
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (msg) {
            var responseFromServer = msg.d;
            alert(responseFromServer);
        },
        error: function (e) {
            alert("There was an issue with the web service connection.");
        }
    });
}

function getEmails() {
    var options =
    {
        url: ResolveUrl("~/home/emails"),
        type: "POST",
        data: { __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').first().val() },
        success: function (status) {
            var list = $("#emailData").empty();
            $.each(status, function (index, item) { list.append($("<div>").text(item)); });
        },
        error: function (info) {
            alert(info);
        }
    };

    $.ajax(options);
}

$(getEmails);
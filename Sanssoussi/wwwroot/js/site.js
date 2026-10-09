// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

var baseUrl = document.body.getAttribute("data-base-url");

function ResolveUrl(url) {
    if (url.indexOf("~/") == 0) {
        url = baseUrl + url.substring(2);
    }
    return url;
}

function AddComments() {
    if ($("#NewComment:visible").length < 1) {
        $("#NewComment").show();
        $("#NewCommentsBtn").val("Ajouter");
    }
    else {
        $.ajax({
                url: ResolveUrl("~/home/comments"),
                type: "POST",
                data: {
                    comment: $("#NewComment").val(),
                    __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').first().val()
                },
                success: function (status) {
                    if (status != "success") {
                        alert(status);
                    }

                    $("#NewComment").hide();
                    $("#NewCommentsBtn").val("Nouveau commentaire");
                },
                error: function (info) {
                    alert(info);
                }
            }
        );
    }
}

function search() {
    window.location = ResolveUrl("~/home/Search?searchData=" + $("#searchBox").val());
}

$(function () {
    $("#NewCommentsBtn").on("click", AddComments);
    $("#searchsBtn").on("click", search);
});

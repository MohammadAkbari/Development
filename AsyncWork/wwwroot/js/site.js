(function ($) {

    var opertationOnClick = function () {

        var $this = $(this);

        var url = $this.data("url");
        var title = $this.text();

        $.ajax(url, {
            type: "POST",
            //data: { id: 10, name: "ten" }
        })
        .done(function (response) {

            var message = JSON.stringify(response);
            var time = new Date().toLocaleString();
            var html = title + " : " + message + " : " + time;

            var div = document.createElement('div');

            $(div).html(html)
                .addClass("alert alert-success")
                .appendTo($("#response"));
        })
        .fail(function () {
            console.log("error");
        })
        .always(function () {
            console.log("complete");
        });
    };

    $(function () {
        $(".opertation").on("click", opertationOnClick);
    });

})(window.jQuery);


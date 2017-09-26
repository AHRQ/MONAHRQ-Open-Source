/**
 * Created by toufika.rahman on 5/5/2017.
 */
/*mobile view -menu icon toggle*/
function toggleIcon(element) {
    $(element).toggleClass("active");
    $('.nav--header .navbar-nav li').each(function (index) {
        $(this).click(function () {
            $(element).trigger("click");
        });
    })
}

function transferToDesktopView() {
    window.location = window.location.href.replace("index-mobile.html", "index.html?desktop=1");
}

function switchToMedicalGroup() {
    $('#generalInfo').removeClass('active');
    $('#medicalGroupRatings').addClass('active');
}

function showFootNotes(element) {
    $(element).closest('.info-graphic-foot-notes').toggleClass("active");
}

function gotoFootnote() {
    if (!$('#accordionFooteNotes').closest('.info-graphic-foot-notes').hasClass('active'))
        $('#accordionFooteNotes').closest('.info-graphic-foot-notes').toggleClass("active");
    $("html, body").scrollTop($('#accordionFooteNotes').offset().top);
}
/*.mobile view -menu icon toggle*/
$(document).ready(function () {
    $('.navbar-nav li').each(function (index) {
        console.log($(this));
    });
});

$(function () {
    $('#accordion').on('shown.bs.collapse', function (e) {
        var offset = $(this).find('.collapse.in').prev('.panel-heading');
        if(offset) {
            $('html,body').animate({
                scrollTop: $(offset).offset().top -20
            }, 500);
        }
    });
});

$(document).ready(function () {
    $('a[data-toggle="tab"]').on('shown.bs.tab', function (e) {
        $('a[data-toggle="tab"]').find('.eclipse-text-doctor').next().hide();
        $(this).find('.eclipse-text-doctor-full').show();
    })
});
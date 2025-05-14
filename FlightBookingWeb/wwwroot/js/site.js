$(document).ready(function () { 
        // Initialize the date picker
        $('#datepicker').datepicker({
            format: 'mm/dd/yyyy',
            autoclose: true
        });

        // Initialize the time picker
        $('#timepicker').timepicker({
            timeFormat: 'h:mm p',
            interval: 30,
            minTime: '0',
            maxTime: '23:00',
            defaultTime: 'now',
            startTime: '0',
            dynamic: false,
            dropdown: true,
            scrollbar: true
        });
        // Initialize the image hover icon
        $('#image-hover-icon').hover(function () {
            $(this).css('opacity', '1');
        }, function () {
            $(this).css('opacity', '0.0');
        });
        // Initialize the image hover icon click event
        $('#image-hover-icon').click(function () {
            alert('Image clicked!');
        });
        // Initialize the image hover icon position
        $(window).on('mousemove', function (e) {
            $('#image-hover-icon').css({
                left: e.pageX + 10,
                top: e.pageY + 10
            });
        });
        // Initialize the image hover icon show/hide
        $(document).on('mouseenter', '#image-hover-icon', function () {
            $(this).fadeIn();
        }).on('mouseleave', '#image-hover-icon', function () {
            $(this).fadeOut();
        });
        // Initialize the image hover icon show/hide on scroll
        $(window).on('scroll', function () {
            if ($(this).scrollTop() > 100) {
                $('#image-hover-icon').fadeIn();
            } else {
                $('#image-hover-icon').fadeOut();
            }
        });
        // Initialize the image hover icon show/hide on load
        $(window).on('load', function () {
            $('#image-hover-icon').fadeIn();
        });
        // Initialize the image hover icon show/hide on resize
        $(window).on('resize', function () {
            $('#image-hover-icon').fadeIn();
        });
        // Initialize the image hover icon show/hide on focus
        $(document).on('focus', '#image-hover-icon', function () {
            $(this).fadeIn();
        }).on('blur', '#image-hover-icon', function () {
            $(this).fadeOut();
        });
        // Initialize the image hover icon show/hide on keydown
        $(document).on('keydown', '#image-hover-icon', function () {
            $(this).fadeIn();
        }).on('keyup', '#image-hover-icon', function () {
            $(this).fadeOut();
        });
        // Initialize the image hover icon show/hide on keypress
        $(document).on('keypress', '#image-hover-icon', function () {
            $(this).fadeIn();
        }).on('keyup', '#image-hover-icon', function () {
            $(this).fadeOut();
        });
        // Initialize the image hover icon show/hide on keyup
        $(document).on('keyup', '#image-hover-icon', function () {
            $(this).fadeIn();
        }).on('keyup', '#image-hover-icon', function () {
            $(this).fadeOut();
        });
        // Set up footer
        $(window).scroll(function () {
            var footer = $('footer.bg-white');
            var windowBottom = $(window).scrollTop() + $(window).height();
            var footerTop = footer.offset().top;

            // Khi footer xuất hiện trong viewport
            if (windowBottom >= footerTop) {
                // Thêm class sal-animate và thay đổi data-sal
                footer
                    .addClass('sal-animate')
                    .attr('data-sal', 'slide-up');

                // Optional: Remove scroll event after triggering once
                $(window).off('scroll');
            }
        });
    });

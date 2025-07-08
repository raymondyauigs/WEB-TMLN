$(document).ready(function () {
    /*
    ec methods
    //refetchEvents()
    //getOption(name) i.e. getOption('date) //current date
    //setOption(name,value)
    
    */
    var eventurl = $('#eventcal').attr('urlis');
    var eventediturl = $('#eventcal').attr('editurlis');
    var eventnewurl = $('#eventcal').attr('newurlis');
    var holurl = $('#eventcal').attr('holurlis');
    var usertag = $('#eventcal').attr('usertag');
    var newable = $('#eventcal').attr('newable');
    let ec;

    ec = new EventCalendar(document.getElementById("eventcal"), {
        //customButton: { button1: { text:, click: } }
        //resources: [{id,title,eventBackgroundColor,eventTextColor}] ,describe the resource text/background color
        //dateClick: fn(info:{date,allDay,dayEl,jsEvent,view,resource})
        dateClick: function (info) {
            
            if (!info.jsEvent.ctrlKey) {
                return false;
            }
            if (moment().diff(info.date, 'days') > 0) {
                alertLib.Core.alert('New Reservation Error', 'Could not new Record at past date!', 'OK');
                return false;
            }
            if ($(info.dayEl).hasClass('ec-holiday') || $(info.dayEl).hasClass('ec-sun')) {
                alertLib.Core.alert('New Reservation Error', 'Could not new Record at holiday date!', 'OK');
                return false;
            }
            window.location.assign(eventnewurl + "/?date=" + moment(info.date).format('DD/MM/YYYY'));
            
        },
        //selectable:
        loading: function (isLoading) {
            if (isLoading) {
                $('body').mLoading({ text: 'loading reservations ...' });

            }
            else {
                $('body').mLoading('hide');
                var $ec = ec;
                $('.ec-content .ec-days .ec-day').removeClass('.ec-holiday');
                $('.ec-content .ec-days .ec-day time.is-holiday').find('span').remove();
                $('.ec-content .ec-days .ec-day time.is-holiday').removeClass('is-holiday');
                if ($ec) {
                    var $view = $ec.getView();
                    if ($view) {

                        $.get(holurl + '/' + moment($view.currentStart).format('YYYY/MM') + '/true', function (r) {
                            if (r && r.length) {
                                $.each(r, function (i, hol) {
                                    var holparts = hol.split('!');
                                    var $holday = $('.ec-content .ec-days [datetime="' + holparts[0] + '"]').parent('.ec-day');
                                    $holday.addClass('ec-holiday');
                                    $holday.find('time').prepend('<span>' + holparts[1] + '</span>');
                                    $holday.find('time').addClass('is-holiday');

                                });
                            }

                        });
                    }
                }

            }
        },
        customButtons: {
            'new': {
                text: 'New Reservation',
                click: function (e) {

                    window.location.assign(eventnewurl);
                    return false;

                }

            }
        },
        locale: 'en',
        eventClick: function (info) {
            console.log(`event click `, info);
            window.location.assign(eventediturl + "/?id=" + info.event.id + '&fromCal=' + true);
            return false;

        },
        eventContent: function (info) {
            let start = info.event.start;
            let end = info.event.end;
            let content = ' ' + info.event.title;
            return moment(start).format('HH:mm') + ' - ' + moment(end).format('HH:mm') + content;
        },
        buttonText: { today: 'Today' },
        headerToolbar: {
            start: newable ? "today new" : "today",
            center: "prev title next",
            end: "",
        },
        //other view = 'dayGridMonth', 'listDay', 'listWeek', 'listMonth', 'listYear', 'resourceTimelineDay', 'resourceTimelineWeek', 
        //'resourceTimeGridDay', 'resourceTimeGridWeek', 'timeGridDay' or 'timeGridWeek'.
        // view object return {type,title,currentStart,currentEnd,activeStart,activeEnd}

        view: "dayGridMonth",
        //views: {viewname:{ pointer,slotMinTime,slotMaxTime,slotWidth,resources:[{id,title}]}}
        //resource is a grouping under week list view , using with view - resourceTimeGridWeek
        eventSources: [{
            events: function (info, successfn, failurefn) {

                let fromDate = moment(info.start).format("YYYYMMDD");
                let toDate = moment(info.end).format("YYYYMMDD");

                $.get(eventurl + '/' + usertag + '/' + fromDate + '/' + toDate, function (r) {
                    successfn(r);
                })

            }
        }],
        // eventTimeFormat: function(start,end){
        //     return moment(start).format('HH:mm') + ' - ' + moment(end).format('HH:mm');
        // },    

        //events: [],
        //events: [{id,resourceids|resourceId,allDay,start,end,title,editable,startEditable,durationEditable,backgroundColor,textColor}]
        //eventClassNames: fn(info:{event,view})
        //eventClick: fn(info:{el,event,jsEvent,view})
        //eventContent: fn(info:{event,timeText}) => Content (html text or string)
        //eventMouseEnter: fn(info:{el,event,jsEvent,view})
        //eventMouseLeave: fn(info:{el,event,jsEvent,view})
        //eventSources: fn(info:{start,end,startStr,endStr},successfn:(events[]) => void,failurefn: (error) => void)
        //height: css value of calendar height
        //loading:fn(isLoading:boolean)
        //nowIndicator: {boolean} give the line mark current time in timeGrid
        //resourceLabelContent: fn(info:{resource,date})
        //select:fn(info:{start,end,starstr,endstr,allDay,jsEvent,view,resource})
    });
});

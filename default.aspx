<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="default.aspx.cs" Inherits="ovcs._default" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <link rel="stylesheet" href="css/ovcs.css" />
    <link rel="stylesheet" href="css/login.css" />
    <script src="https://ajax.googleapis.com/ajax/libs/jquery/3.2.1/jquery.min.js"></script>
    <script src="map.ts"></script>
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title></title>
    <style>
        #droneInstrumentPanel {
            position: absolute;
            top: 10px;
            left: 10px;
            z-index: 99;
        }

        #droneInstrumentPanelContainer {
            width: 100px;
            height: 40px;
            position: relative;
        }

        #droneSpeedImage {
            position: absolute;
            left: 0;
            top: 0;
        }

        #droneSpeedText {
            z-index: 100;
            position: absolute;
            color: white;
            font-size: 24px;
            font-weight: bold;
            left: 0px;
            top: 0px;
        }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <asp:Panel ID="pnlLogin" runat="server" CssClass="login">
            <div class="form__field">
                <label for="login__username">

                    <span class="icon">
                        <img src="images/user.png" class="icon_img" />
                    </span><span class="hidden">Username</span></label>

                <asp:TextBox ID="txtUsername" runat="server" CssClass="form__input"></asp:TextBox>
            </div>
            <div class="form__field">
                <label for="login__password">

                    <span class="icon">
                        <img src="images/password.png" class="icon_img" />
                    </span><span class="hidden">Password</span></label>

                <asp:TextBox ID="txtPassword" runat="server" TextMode="Password" CssClass="form__input"></asp:TextBox>
            </div>
            <div class="form__field">
                <asp:Button ID="btnLogin" runat="server" Text="Login" OnClick="btnLogin_Click" />
            </div>
        </asp:Panel>
        <asp:Panel ID="pnlMap" CssClass="map" runat="server" Style="float: left;">
            <script>
                var webAPIUrl = '<%= WebAPIUrl %>';
                var pilot;
                var drone;
                var pilot_coords = { lat: parseFloat('<%= Pilot.Lat %>'.replace(',', '.')), lng: parseFloat('<%= Pilot.Lon %>'.replace(',', '.')) };
                var drone_coords = { lat: parseFloat('<%= Drone.Lat %>'.replace(',', '.')), lng: parseFloat('<%= Drone.Lon %>'.replace(',', '.')) };
                var infowindow;
                var view;
                //var map;
                function initMap() {
                    var mapStyles = [<%= MapStyles %>];
                    map = new google.maps.Map(document.getElementById('pnlMap'), {
                        center: pilot_coords,
                        zoom: 15,
                        scrollwheel: false,
                        draggable: false,
                        disableDefaultUI: true,
                        styles: mapStyles
                    });
                    map.addListener('click', function (event) {
                        if (isInfoWindowOpen(infowindow)) {
                            infowindow.close();
                        } else {
                            var contentIW = '<div class="infowindow">' +
                                '<h2>actions</h2>' +
                                '<hr>' +
                                '<div class="zoompanel">' +
                                '<h3>zoom</h3>' +
                                '<a href="javascript:zoomTo(20);infowindow.close();">LOW</a>&nbsp;<a href="javascript:zoomTo(14);infowindow.close();">HIGH</a><br />' +
                                '<a href="javascript:zoomChange(1);infowindow.close();"><img src="images/plus.png" class="icon_01"></a>&nbsp;<a href="javascript:zoomChange(-1);infowindow.close();"><img src="images/minus.png" class="icon_01"></a>' +
                                '</div>' +
                                '<hr>' +
                                '<div class="movepanel">' +
                                '<h3>move drone</h3>' +
                                '<a href="javascript:moveDrone2([' + event.latLng.lat() + ', ' + event.latLng.lng() + ']);infowindow.close();"><img src="images/move.png" class="icon_02"></a>&nbsp;&nbsp;' +
                                '<a href="javascript:moveDrone2([' + pilot_coords.lat + ', ' + pilot_coords.lng + ']);infowindow.close();"><img src="images/pilot.png" class="icon_02"></a>&nbsp;&nbsp;' +
                                '<a href="javascript:stopDrone();infowindow.close();"><img src="images/pause.png" class="icon_02"></a>' +
                                '</div>' +
                                '<hr>' +
                                '<div class="inventorypanel">' +
                                '<h3>inventory</h3>' +
                                '</div>' +
                                '</div>';
                            infowindow.setContent(contentIW);
                            infowindow.open(map, map);
                            infowindow.setPosition(event.latLng);
                            setInfowindowProps('#ccc', 'false', 'images/close.png');
                        }
                    });
                    function isInfoWindowOpen(infoWindow){
                        var iw_map = infoWindow.getMap();
                        return (iw_map !== null && typeof iw_map !== "undefined");
                    }
                    //// The markers param is an array of markers
                    //var markerCluster = new MarkerClusterer(map, markers,
                    //    { imagePath: 'images/cluster.png' }
                    //);
                    function setHeader(xhr) {
                        xhr.setRequestHeader('Access-Control-Allow-Origin', '*');
                    }
                    infowindow = new google.maps.InfoWindow({});
                    function displayComponents(data) {
                        console.log('Executing: displayComponents()');
                        infowindow.open(map, marker);
                    }
                    myFuncs['funcPlacePilot']();
                    myFuncs['funcPlaceDrone']();
                    $('#droneInstrumentPanel').hide();
                }
                var lastupdate = new Date;
                var updatetime = 3000; // milliseconds
                var position = [0, 0];
                var speed = 25; // m/s
                var numDeltas = 100;
                var delay = 100; // milliseconds
                var deltaLat;
                var deltaLng;
                var startpos = [0, 0];
                var endpos = [0, 0];
                var keepmoving = 0;
                var minzoom = 14;
                var maxzoom = 20;
                var droneCircle;
                var compsinrange = [];
                function moveDrone2(latLng) {
                    var droneposition = drone.getPosition();
                    startpos[0] = droneposition.lat();
                    startpos[1] = droneposition.lng();
                    endpos[0] = latLng[0];
                    endpos[1] = latLng[1];
                    // Check if drone is moving already
                    if (keepmoving == 0) {
                        keepmoving = 1;
                        if (view == false) {
                            zoomTo(14);
                        }
                        $('#droneSpeedText').text(getCurrentDroneSpeed() + ' m/s');
                        moveDrone();
                    }
                }
                function panTo(panToPilot, panToCoords) {
                    view=panToPilot;
                    map.panTo(panToCoords);
                    if (view == true) {
                        $('#droneInstrumentPanel').hide();
                        zoomTo(15);
                    } else {
                        $('#droneInstrumentPanel').show();
                        $('#droneSpeedText').text(getCurrentDroneSpeed() + ' m/s');
                        zoomTo(14);
                    }
                }
                function zoomTo(gotoLevel) {
                    var i = gotoLevel - map.getZoom();
                    var delta = i / Math.abs(i);
                    while (i != 0) {
                        zoomChange(delta);
                        i -= delta;
                    }
                }
                function zoomChange(delta) {
                    var zoomLevel = map.getZoom() + delta;
                    if (zoomLevel > maxzoom) {
                        zoomLevel = maxzoom;
                    } else if (zoomLevel < minzoom) {
                        zoomLevel = minzoom;
                    }
                    map.setZoom(zoomLevel);
                    checkDroneCircle();
                }
                function checkDroneCircle() {
                    var drawCircle = false;
                    if (view == false) {
                        drawCircle = (map.getZoom() == 20);
                    }
                    if (drawCircle == true) {
                        droneCircle = new google.maps.Circle({
                            strokeColor: '#FF0000',
                            strokeOpacity: 0.8,
                            strokeWeight: 2,
                            fillColor: '#FF0000',
                            fillOpacity: 0.35,
                            map: map,
                            center: drone_coords,
                            radius: 20
                        });
                        getComponents(<%= Drone.ID %>);
                    } else {
                        if (droneCircle != null) {
                            droneCircle.setMap(null);
                        }
                    }
                }
                function moveDrone() {
                    if (keepmoving == 1) {
                        var droneposition = drone.getPosition();
                        position[0] = droneposition.lat();
                        position[1] = droneposition.lng();
                        var distance = getDistanceFromLatLonInKm(position[0], position[1], endpos[0], endpos[1]);
                        numDeltas = parseInt(((distance * 1000) / speed) / (delay / 1000));
                        deltaLat = (endpos[0] - position[0]) / numDeltas;
                        deltaLng = (endpos[1] - position[1]) / numDeltas;
                        position[0] += deltaLat;
                        position[1] += deltaLng;
                        var latlng = new google.maps.LatLng(position[0], position[1]);
                        drone.setPosition(latlng);
                        drone_coords = latlng;
                        // If Drone view then keep map centered on Drone position
                        if (view == false) {
                            map.panTo(latlng);
                        }
                        if (numDeltas > 1) {
                            setTimeout(moveDrone, delay);
                        } else {
                            keepmoving = 0;
                            zoomTo(15);
                            $('#droneSpeedText').text(getCurrentDroneSpeed() + ' m/s');
                        }
                        // Update db every 'updatetime' milliseconds and when done moving
                        if (keepmoving == 0 || ((new Date) - lastupdate) > updatetime) {
                            myFuncs['funcSendDronePosition']();
                            lastupdate = new Date;
                        }
                    }
                }
                function stopDrone() {
                    keepmoving = 0;
                }
                function getCurrentDroneSpeed() {
                    if (keepmoving == 1) {
                        return speed;
                    } else return 0;
                }
                function getComponents(id) {
                    $.ajax({
                        type: 'GET',
                        url: '<%= WebAPIUrl %>cir/' + id,
                        contentType: 'text/plain',
                        crossDomain: true,
                        success: function (data) {
                            removeComponents();                            
                            $.each(data, function (i, item) {
                                var contentString = '<div>Lat: ' + item.Lat + ', Lon: ' + item.Lon +
                                                    '<br /><img src="images/sun-128.png" style="width: 20px; height: 20px;">' +
                                                    '<hr>' +
                                                    '<div class="inventorypanel">' +
                                                    '<h3>inventory</h3>' +
                                                    '</div>' +
                                                    '</div>';
                                var newMarker = new google.maps.Marker({
                                    position: new google.maps.LatLng(item.Lat, item.Lon),
                                    map: map,
                                    icon: 'images/star.png',
                                    info: contentString
                                });
                                newMarker.addListener('click', function () {
                                    infowindow.setContent(this.info);
                                    infowindow.open(map, newMarker);
                                });
                                compsinrange.push(newMarker);
                            });
                        },
                        error: function (jqXHR, textStatus, errorThrown) {
                            console.log('Error: ' + textStatus + ', thrown: ' + errorThrown);
                        }
                    });
                }
                function removeComponents() {
                    compsinrange.forEach(function(item, index, array) {
                        item.setMap(null);
                    });
                    compsinrange = [];
                }
                function placeComponents(data) {
                    console.log('Executing placeComponents');
                    $.each(data, function (i, item) {
                        var contentString = '<div>Lat: ' + item.Lat + ', Lon: ' + item.Lon +
                                            '<br /><img src="images/sun-128.png" style="width: 20px; height: 20px;">' +
                                            '</div>';
                        var newMarker = new google.maps.Marker({
                            position: new google.maps.LatLng(item.Lat, item.Lon),
                            map: map,
                            icon: 'images/star.png',
                            info: contentString
                        });
                        newMarker.addListener('click', function () {
                            infowindow.setContent(this.info);
                            infowindow.open(map, newMarker);
                        });
                    });
                }
                function getDistanceFromLatLonInKm(lat1, lon1, lat2, lon2) {
                    var R = 6371; // Radius of the earth in km
                    var dLat = deg2rad(lat2 - lat1);  // deg2rad below
                    var dLon = deg2rad(lon2 - lon1);
                    var a =
                      Math.sin(dLat / 2) * Math.sin(dLat / 2) +
                      Math.cos(deg2rad(lat1)) * Math.cos(deg2rad(lat2)) *
                      Math.sin(dLon / 2) * Math.sin(dLon / 2)
                    ;
                    var c = 2 * Math.atan2(Math.sqrt(a), Math.sqrt(1 - a));
                    var d = R * c; // Distance in km
                    return d;
                }
                function deg2rad(deg) {
                    return deg * (Math.PI / 180)
                }
                function setInfowindowProps(bgColor, point, closeImage) {
                    // Find DIV with .gm-style class and navigate from there
                    var iw = $('.gm-style > div:nth-child(1) > div:nth-child(4) > div:nth-child(4) > div:nth-child(1)');
                    iw.addClass('infoWindow');
                    var iw_1_2 = $('.infoWindow > div:nth-child(1) > div:nth-child(2)');
                    iw_1_2.addClass('iw_1_2');
                    iw_1_2.css('background-color', bgColor);			
                    var iw_1_1 = $('.infoWindow > div:nth-child(1) > div:nth-child(1)');
                    var iw_1_3 = $('.infoWindow > div:nth-child(1) > div:nth-child(3)');
                    if(point == 'true') {
                        iw_1_1.addClass('iw_1_1');
                        iw_1_3.addClass('iw_1_3');
                    } else {
                        iw_1_1.addClass('iw_1_1_nopoint');
                        iw_1_3.addClass('iw_1_3_nopoint');
                    }
                    var iw_1_3_1_1 = $('.iw_1_3 > div:nth-child(1) > div:nth-child(1)');
                    iw_1_3_1_1.addClass('iw_1_3_1_1');
                    iw_1_3_1_1.css('background-color', bgColor);			
                    var iw_1_3_2_1 = $('.iw_1_3 > div:nth-child(2) > div:nth-child(1)');
                    iw_1_3_2_1.addClass('iw_1_3_2_1');
                    iw_1_3_2_1.css('background-color', bgColor);			
                    var iw_1_4 = $('.infoWindow > div:nth-child(1) > div:nth-child(4)');
                    iw_1_4.addClass('iw_1_4');
                    iw_1_4.css('background-color', bgColor);
                    var iw_3 = $('.infoWindow > div:nth-child(3)');
                    iw_3.addClass('iw_3');
                    iw_3.css('background-color', bgColor);
                    if(typeof closeImage !== 'undefined') {
                        iw_3.html('<img src="' + closeImage + '" width="13px" height="13px" />');	  
                    } else {
                        iw_3.html('X');	  
                    }
                }
                var myFuncs = {
                    funcEmpty: function Empty() { console.log('Just Empty'); },
                    funcPlacePilot: function PlacePilot() {
                        pilot = new google.maps.Marker({
                            position: pilot_coords,
                            map: map,
                            icon: 'images/pilot.png',
                            customInfo: 'drone:<%= Drone.ID %>;'
                        });
                        pilot.addListener('click', function () {
                            var contentIW = '<div class="infowindow">' +
                                    '<h2>actions</h2>' +
                                    '<hr>' +
                                    '<div class="actionpanel">' +
                                    '<ul>' +
                                    '<li><a href="javascript:panTo(false, drone_coords);infowindow.close();">pan to drone</a></li>' +
                                    '<li><a href="javascript:getComponents(<%= Pilot.ID %>);infowindow.close();">show components</a></li>' +
                                    '</div>';
                            infowindow.setContent(contentIW);
                            infowindow.open(map, pilot);
                            setInfowindowProps('#ccc', 'true', 'images/close.png');
                        });
                    },
                    funcPlaceDrone: function PlaceDrone() {
                        drone = new google.maps.Marker({
                            position: drone_coords,
                            map: map,
                            icon: 'images/drone.png',
                            customInfo: 'pilot:<%= Pilot.ID %>;'
                        });
                        drone.addListener('click', function () {
                            var contentIW = '<div class="infowindow">' +
                                    '<h2>actions</h2>' +
                                    '<hr>' +
                                    '<div class="zoompanel">' +
                                    '<ul>' +
                                    '<li><a href="javascript:panTo(true, pilot_coords);infowindow.close();">pan to pilot</a></li>' +
                                    '<li><a href="javascript:getComponents(<%= Drone.ID %>);infowindow.close();">show components</a></li>' +
                                    '</div>';
                            infowindow.setContent(contentIW);
                            infowindow.open(map, drone);
                            setInfowindowProps('#ccc', 'true', 'images/close.png');
                        });
                    },
                    funcSendDronePosition: function sendDronePosition() {
                        var droneposition = drone.getPosition();
                        position[0] = droneposition.lat();
                        position[1] = droneposition.lng();
                        $.ajax({
                            type: 'PUT',
                            url: '<%= WebAPIUrl %>component/<%= Drone.ID %>',
                            contentType: 'application/json; charset=utf-8',
                            data: JSON.stringify({ 'ID': <%= Drone.ID %>, 'Type': -1, 'Lon': position[1], 'Lat': position[0], 'DestLon': -1, 'DestLat': -1, 'Heading': -1, 'Speed': -1, 'Parent': -1, 'Visible': 'null', 'Created': 'null', 'Modified': 'null', 'Deleted': 'null' }),
                            dataType: 'json',
                            //crossDomain: true,
                            success: function (data) {
                                // Here's where you handle a successful response.
                                console.log('New position successfuly sent to db');
                            },
                            error: function (jqXHR, textStatus, errorThrown) {
                                console.log('Error: ' + textStatus + ', thrown: ' + errorThrown);
                            }
                        });
                    },
                    funcJSONCallback: function jsoncallback(data) {
                        $.each(data, function (i, item) {
                            var contentString = '<div>Lat: ' + item.Lat + ', Lon: ' + item.Lon +
                                                '<br /><img src="images/sun-128.png" style="width: 20px; height: 20px;">' +
                                                '</div>';
                            var infowindow = new google.maps.InfoWindow({
                            });
                            var newMarker = new google.maps.Marker({
                                position: new google.maps.LatLng(item.Lat, item.Lon),
                                map: map,
                                icon: 'images/drone.png',
                                info: contentString
                            });
                            newMarker.addListener('click', function () {
                                infowindow.setContent(this.info);
                                infowindow.open(map, newMarker);
                            });
                        });
                    }
                };
                // Create the XHR object.
                function createCORSRequest(method, url) {
                    var xhr = new XMLHttpRequest();
                    if ("withCredentials" in xhr) {
                        // XHR for Chrome/Firefox/Opera/Safari.
                        xhr.open(method, url, true);
                    } else if (typeof XDomainRequest != "undefined") {
                        // XDomainRequest for IE.
                        xhr = new XDomainRequest();
                        xhr.open(method, url);
                    } else {
                        // CORS not supported.
                        xhr = null;
                    }
                    return xhr;
                }

                // Helper method to parse the title tag from the response.
                function getTitle(text) {
                    return text.match('<title>(.*)?</title>')[1];
                }

                // Make the actual CORS request.
                function makeCorsRequest() {
                    // This is a sample server that supports CORS.
                    var url = 'http://html5rocks-cors.s3-website-us-east-1.amazonaws.com/index.html';

                    var xhr = createCORSRequest('GET', url);
                    if (!xhr) {
                        console.log('CORS not supported');
                        return;
                    }

                    // Response handlers.
                    xhr.onload = function () {
                        var text = xhr.responseText;
                        var title = getTitle(text);
                        console.log('Response from CORS request to ' + url + ': ' + title);
                    };

                    xhr.onerror = function () {
                        console.log('Woops, there was an error making the request.');
                    };

                    xhr.send();
                }
            </script>
            <script src="https://maps.googleapis.com/maps/api/js?key=<%= APIKey %>&callback=initMap" async defer></script>
        </asp:Panel>
        <asp:Panel ID="droneInstrumentPanel" runat="server">
            <div id="droneInstrumentPanelContainer">
                <img id="droneSpeedImage" src="images/speed.png" />
                <p id="droneSpeedText">
                </p>
            </div>
        </asp:Panel>
    </form>
</body>
</html>

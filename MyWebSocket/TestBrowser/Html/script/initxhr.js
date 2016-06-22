function init() {
    var body = document.body;
    while (body.childNodes.length > 0) {
        body.removeChild(body.childNodes[0]);
    }
    var win = {
        width: document.documentElement.clientWidth - 32
    }
    var info = document.getElementById("info");
    var elem = document.getElementById("elem");
    var text = document.getElementById("text");
    var send = document.getElementById("send");
    if (info === null) {
        info = document.createElement('div');
        info.id = 'info';
        body.appendChild(info);
    }
    info.style.width = win.width + 'px';
    info.style.margin = 'auto';
    info.style.height = '20px';

    if (elem === null) {
        elem = document.createElement('div');
        elem.id = 'elem';
        body.appendChild(elem);
    }
    elem.style.width = win.width + 'px';
    elem.style.margin = 'auto';
    elem.style.height = '600px';
    if (text === null) {
        text = document.createElement('textarea');
        text.id = 'text';
        body.appendChild(text);
    }
    text.style.width = win.width + 'px';
    text.style.margin = 'auto';
    text.style.height = '60px';
    text.style.display = 'block';
    if (send === null) {
        send = document.createElement('button');
        send.id = 'send';
        body.appendChild(send);
    }
    send.style.width = win.width + 'px';
    send.style.margin = 'auto';
    send.style.height = '20px';
    send.style.display = 'block';
}
function initxhr() {
    init();
    subscribe();
    // ����� �������
    var str = 'http://12.0.0.1:8081/message';
    var elem = document.getElementById("elem");
    var text = document.getElementById("text");
    var send = document.getElementById("send");

    var _xhr = new XMLHttpRequest();
        send.onclick = function () {
            _xhr.open('GET', str, true);
            _xhr.send(_text.value);
                      _text.value = '';
    }
}
function subscribe() {
    var str = 'http://12.0.0.1:8081/subscribe';
    var info = document.getElementById("info");
    var elem = document.getElementById("elem");

    var _xhr = new XMLHttpRequest();
        _xhr.open('GET', str, true);
        info.innerText = "��������";
        _xhr.onreadystatechange = function () {
        if (_xhr.readyState === 4) {
            if (_xhr.status === 200) {
                var div = document.createElement('div');
                div.style.backgroundColor = 'yellow';
                div.innerText = _xhr.responseText;
                elem.appendChild(div);
            }
            info.innerText = "��������";
            setTimeout(function () {
                _xhr.open('GET', str, true);
                info.innerText = "��������";
                _xhr.send();
            }, 1000);
        }
    }
        _xhr.send(null);
}
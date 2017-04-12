// Write your Javascript code.
function Reload()
{
    $.get("home/GetChart", {}, function(data)
    {
        document.getElementById("test").innerHTML = data;
        console.log(123);
    });
    console.log(1);
}
setInterval(Reload, 1000);
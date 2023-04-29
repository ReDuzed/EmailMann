<?php 
    $status = "Error";
    $mode = '';     // Collect, Send, MassSend
    $service = '';
    $port = '';
    $username = '';
    $password = '';
    $hostaddr = '';
    $subject = '';
    $message = '"Example message"';
    $ssl = 'true';
    $character = '';
    $recieve = $_POST['address'];
    $character = $_POST['username'];
    $args = "-mode $mode -service $service -port $port -username $username -password $password -hostaddress $hostaddr -subject $subject -enablessl $ssl --character $character --collectaddress $recieve -message $message";
    $sock = socket_create(AF_INET, SOCK_STREAM, SOL_TCP);
    socket_connect($sock, 'localhost', 8080);
    socket_write($sock, $args, strlen($args));
    socket_close($sock);
    $status = "Success";
    // Route back to index.html
    //header("Location: index.html");
?>
<!DOCTYPE html>

<html lang="en-us" xmlns="http://www.w3.org/1999/xhtml">
<head>
    <meta charset="utf-8" />
    <title>Details</title>
    <link rel="stylesheet" type="text/css" href="styles.css" />
</head>
<body>
    <div class="stats" style="font-family: 'Courier New', Courier, monospace">
        <span class="font">
            <?php
                echo $status;
            ?>
        </span>
    </div>
</body>
</html>

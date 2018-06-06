<?php
require_once('./preload.php');

unset($_SESSION['user']);
session_destroy();

header("Location: ./index.php");
?>
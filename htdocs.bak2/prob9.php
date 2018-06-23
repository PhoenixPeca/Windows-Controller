<form method="get">
	num1: <input type="text" name="num1"/>
	<br>
	num2: <input type="text" name="num2"/><br>
	<input type="submit"/>
</form>
<br><br><br>
<?php


if(isset($_GET['num1']) && isset($_GET['num2'])) {
	$num1 = $_GET['num1'];
	$num2 = $_GET['num2'];
	if($num1 != $num2) {
		echo "$num1 is not equal to $num2<br>";
	}
	if($num1 > $num2) {
		echo "$num1 is greater than $num2<br>";
	}
	if($num1 == $num2) {
		echo "$num1 is equal to $num2<br>";
	}
	if($num1 < $num2) {
		echo "$num1 is less than $num2<br>";
	}
	
}

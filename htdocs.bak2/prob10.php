<form method="get">
	input any numbers: <input type="text" name="num1"/>
	<input type="submit"/>
</form>
<br><br><br>


<?php
if(isset($_GET['num1'])) {
	$input = intval($_GET['num1']);
	$n = str_split($_GET['num1']);
	$number = 0;
	foreach($n as $item) {
		$number = $number+intval($item);
	}
	
	echo "The sum of the digits of the number $input is: $number";
}

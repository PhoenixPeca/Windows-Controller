<form method="get">
	input number of terms: <input type="text" name="num1"/>
	<input type="submit"/>
</form>
<br><br><br>


<?php
if(isset($_GET['num1'])) {
	$v = intval($_GET['num1']);
	$t = 0;
	for($x=1; $x<=$v; $x++) {
		for($y=1; $y<=$x; $y++) {
			echo "1";
		}
		if($y < $v+1) {
			echo "+";
		}
		$t = $t+$x;
		$b[] = $x;
	}
	
	echo "<br><br><br><br>The sum of the series is: ";
	
	foreach($b as $item) {
		echo $item;		
	}
}

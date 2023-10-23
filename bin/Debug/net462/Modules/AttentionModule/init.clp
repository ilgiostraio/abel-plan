;DEFINITION OF GLOBAL VARIABLES

(defglobal ?*xfisso* = 0.5) 
(defglobal ?*yfisso* = 0.5) 

(defglobal ?*KinectFOV_hor* = 0.7339) 
(defglobal ?*KinectFOV_ver* = 0.4695) 
(defglobal ?*pi* = 3.14159)


(defglobal ?*s* = 0.65) 
(defglobal ?*sm_list* = (create$))

(defglobal ?*speaking_probability* = 0.0)


;DEFINITION OF TEMPLATES

;--------------------------------------------------------------SUBJECT TEMPLATE

(deftemplate MAIN::subject "These are subject's peculiar 
							characteristics detected by the Robot"
   (slot id (type INTEGER) (default ?DERIVE))
   (slot idKinect (type INTEGER) (default ?DERIVE))
   (slot trackedState (type SYMBOL) (default False) (allowed-symbols False True))
   (multislot name (type LEXEME) (default unknown))
   (slot gender (type SYMBOL) (default unknown) (allowed-symbols Male Female unknown))
   (slot age (type INTEGER) (default ?DERIVE))
   (slot speak_prob)
   (slot gesture (type INTEGER) (default 0))
   (slot uptime (type NUMBER) (default ?DERIVE) (range 0.0 ?VARIABLE))
   (slot angle (type NUMBER) (default ?DERIVE))
   
   (slot happiness_ratio (type NUMBER) (default ?DERIVE))
   (slot anger_ratio (type NUMBER) (default ?DERIVE))
   (slot sadness_ratio (type NUMBER) (default ?DERIVE))
   (slot surprise_ratio (type NUMBER) (default ?DERIVE))
   
   (multislot head (type NUMBER) (default ?DERIVE))
   (multislot neck (type NUMBER) (default ?DERIVE))
   (multislot spineShoulder (type NUMBER) (default ?DERIVE))
   (multislot spineBase (type NUMBER) (default ?DERIVE))
   (multislot spineMid (type NUMBER) (default ?DERIVE))
   (multislot shoulderLeft (type NUMBER) (default ?DERIVE))
   (multislot shoulderRight (type NUMBER) (default ?DERIVE))
   (multislot elbowLeft (type NUMBER) (default ?DERIVE))
   (multislot elbowRight (type NUMBER) (default ?DERIVE))
   (multislot wristLeft (type NUMBER) (default ?DERIVE))
   (multislot wristRight (type NUMBER) (default ?DERIVE))
   (multislot handLeft (type NUMBER) (default ?DERIVE))
   (multislot handRight (type NUMBER) (default ?DERIVE))
   (multislot hipLeft (type NUMBER) (default ?DERIVE))
   (multislot hipRight (type NUMBER) (default ?DERIVE))
   (multislot kneeLeft (type NUMBER) (default ?DERIVE))
   (multislot kneeRight (type NUMBER) (default ?DERIVE))
   (multislot ankleLeft (type NUMBER) (default ?DERIVE))
   (multislot ankleRight (type NUMBER) (default ?DERIVE))
   (multislot footLeft (type NUMBER) (default ?DERIVE))
   (multislot footRight (type NUMBER) (default ?DERIVE))
   (multislot handTipLeft (type NUMBER) (default ?DERIVE))
   (multislot handTipRight (type NUMBER) (default ?DERIVE))
   (multislot thumbLeft (type NUMBER) (default ?DERIVE))
   (multislot thumbRight (type NUMBER) (default ?DERIVE))

   (slot LeftEyeClosed (type LEXEME) (default Unknown) (allowed-symbols Yes No Unknown Maybe))
   (slot RightEyeClosed (type LEXEME) (default Unknown) (allowed-symbols Yes No Unknown Maybe))
   (slot LookingAway (type LEXEME) (default Unknown) (allowed-symbols Yes No Unknown Maybe))
   (slot WearingGlasses (type LEXEME) (default Unknown) (allowed-symbols Yes No Unknown Maybe))
   (slot MouthOpen (type LEXEME) (default Unknown) (allowed-symbols Yes No Unknown Maybe))
   (slot MouthMoved (type LEXEME) (default Unknown) (allowed-symbols Yes No Unknown Maybe))
   (slot Engaged (type LEXEME) (default Unknown) (allowed-symbols Yes No Unknown Maybe))
)

;--------------------------------------------------------------SURROUNDINGS TEMPLATE (ENVIRONMENTAL FEATURES)

(deftemplate MAIN::surroundings "These are surrounding's peculiar 
								 characteristics detected by F.A.C.E."
   (slot soundAngle (type NUMBER) (default ?DERIVE))
   (slot soundEstimatedX (type NUMBER) (default ?DERIVE))
   (slot soundAverageNorm (type NUMBER) (default ?DERIVE))
   (slot soundDecibelNorm (type NUMBER) (default ?DERIVE))
   (multislot recognizedWord (type LEXEME) (default unknown))
   (slot numberSubject (type INTEGER))
   (multislot ambience (type LEXEME))
   (multislot resolution (type NUMBER))
   (multislot saliency (type NUMBER)) ; first two values are x,y in pixels and third one is weight
   (slot toiMic (type NUMBER))
   (slot toiTemp (type NUMBER))
   (slot toiIR (type NUMBER))
   (slot toiTouch (type LEXEME) (default False) (allowed-symbols False True))
   (slot toiLux (type NUMBER))
)

;--------------------------------------------------------------ROBOT INNER PARAMETERS TEMPLATE

(deftemplate MAIN::winner "This is the winner template, inside you can find ID, point to look and lookrule fired"
   (slot id (type INTEGER) (default 0))
   (multislot point (type NUMBER) (default 0.5 0.5 0.0))
   (slot lookrule (type LEXEME) (default none))
)


(deftemplate MAIN::face "This is the robot emotional state template, containing its current mood and expression"
   (multislot mood (type NUMBER) (default 0.0 0.0))
   (multislot ecs (type NUMBER) (default 0.0 0.0))
)


(deftemplate MAIN::sm "This is the somatic marker template"
   (slot id (type INTEGER) (default 0))
   (slot marker (type NUMBER) (default 0.0))
   (multislot bp (type NUMBER) (default 0.0 0.0))
)


(deftemplate MAIN::max_sm "This is the maximum somatic marker template"
   (slot id (type INTEGER) (default 0))
)


;DEFINITION OF FUNCTIONS

(deffunction precision (?num ?digits)
(bind ?m (integer (** 10 ?digits)))
(/ (integer (* ?num ?m)) ?m))

(deffunction find-max (?template ?predicate)
   (bind ?max FALSE)
   (do-for-all-facts ((?f ?template)) TRUE
      (if (or (not ?max) (funcall ?predicate ?f ?max))
         then
         (bind ?max ?f)))
   (return ?max))


(deffunction moodchange "This is the function for changing the mood values without going out from the interval (-1.0 ; 1.0)" 
(?m ?n)
	(bind ?newm (+ ?n ?m))
		(if (and (> ?newm -1.0) (< ?newm 0)) then (return ?newm))
		(if (<= ?newm -1.0) then (return -1.0))
		(if (and (> ?newm 0) (< ?newm 1.0)) then (return ?newm))
		(if (>= ?newm 1.0) then (return 1.0))
)

(deffunction flatmood "This is the function for changing the mood values bringing them to (0.0 ; 0.0)" 
(?m)
	(if (and (> ?m 0.0) (< ?m 0.06)) then (return 0.0))
	(if (= ?m 0.0) then (return 0.0))
	(if (and (< ?m 0.0) (> ?m -0.06)) then (return 0.0))
	(if (< ?m -0.015) then 
			(bind ?newm (+ ?m 0.05))
			(return ?newm))
	(if (> ?m 0.015) then 
			(bind ?newm (- ?m 0.05))
			(return ?newm))
)

(deffunction modulus_sm "This is the function that procecces the modulus of mood valence and arousal values"
(?v ?a)
	(bind ?mod (sqrt (+ (** ?v 2) (** ?a 2) )))
		(if (and (>= ?mod ?*s*)(< ?v 0)) then (return -1))
		(if (and (>= ?mod ?*s*)(> ?v 0)) then (return 1))
		(if (< ?mod ?*s*) then (return 0))
)

(deffunction calib_x "This is the function that starting from xyz coordinates in meters given by the Kinect, normalizes the X coordinate according to the abel fov and taking into account the distance of the point"
(?x_coord ?z_coord)
   (bind ?X_max (* ?z_coord (tan ?*KinectFOV_hor*)))
   (bind ?x (+ 0.5 (* (/ ?x_coord ?X_max) 0.5)))
   (return ?x)
)

(deffunction calib_y "This is the function that starting from xyz coordinates in meters given by the Kinect, normalizes the Y coordinate according to the abel fov and taking into account the distance of the point"
(?y_coord ?z_coord)
   (bind ?Y_max (* ?z_coord (tan ?*KinectFOV_ver*)))
   (bind ?y (+ 0.5 (* (/ ?y_coord ?Y_max) 0.5)))
   (return ?y)
)

(deffunction eu-distance (?x1 ?y1 ?z1 ?x2 ?y2 ?z2) "This rule is able to return the euclidian distance between two points given the x y z coordinates of that points"
   (sqrt (+ (* (- ?x1 ?x2) (- ?x1 ?x2))
            (* (- ?y1 ?y2) (- ?y1 ?y2))
            (* (- ?z1 ?z2) (- ?z1 ?z2)))))

;DEFINITION OF INIT FACTS

(deffacts MAIN::initialization "Just to initialize some useful facts" 
   (winner)
   (winner_not_chosen)
   (face)
   (tracking_is OFF)
   (max_sm)
)

;DEFINITION OF RULES

;--------------------------------------------------------------STANDARD BEHAVIOR
   
(defrule MAIN::check_presence "If, at least, one subject is tracked then tracking is on" 
   (surroundings (numberSubject ?numb))
   ?trackOFF <- (tracking_is OFF)
   (test (> ?numb 0))
=>
   (retract ?trackOFF)
   (assert (tracking_is ON))
)


(defrule MAIN::check_loneliness "If no subject is tracked then tracking is off" 
   (surroundings (numberSubject ?numb))
   (test (eq ?numb 0))
=>
   (assert (tracking_is OFF))
)
 
  
(defrule MAIN::refresh_loneliness "If you were alone at the beginning, you will be alone also when noone is there"
   ?trackON <- (tracking_is ON)
   (tracking_is OFF)
=>
   (retract ?trackON)
)

   
(defrule MAIN::boring_loneliness "If the robot doesn't see anyone is bored and follows the saliency point while the mood calm down to 0,0"
   ?check <- (winner_not_chosen)
   (tracking_is OFF)
   ?surround <- (surroundings (saliency ?x ?y ?) (resolution ?w ?h))
   ?win <- (winner) 
   ?face <- (face (mood ?v ?a))
   =>
   (bind ?nx (/ ?x ?w))
   (bind ?ny (- 0.6 (/ ?y ?h)))
   (bind ?newv (flatmood ?v))
   (bind ?newa (flatmood ?a))
   (modify ?face (mood ?newv ?newa))
   (modify ?win (id 1) (point ?nx ?ny 100) (lookrule LONELINESS))
   (retract ?surround ?check)
   (assert (winner_is_chosen))
)


(defrule MAIN::lookrule_speak "This rule selects the winner as the person who is probably speaking"
   (declare (salience 99))
   ?check <- (winner_not_chosen)
   (tracking_is ON)
   (subject (idKinect ?id) (speak_prob ?prob) (head ?x ?y ?z))
   ?win <- (winner)
   (test (> ?prob 0.98))
   =>
   (bind ?x_cal (calib_x ?x ?z))
   (bind ?y_cal (calib_y ?y ?z))
   (bind ?*speaking_probability* = ?prob)
   (modify ?win (id ?id) (point ?x_cal ?y_cal ?z) (lookrule SPEAK))
   (retract ?check) 
   (assert (winner_is_chosen))
)



(defrule MAIN::lookrule_happyface "This rule selects the winner as the person who is smiling"
   (declare (salience 95))
   ?check <- (winner_not_chosen)
   (tracking_is ON)
   (subject (idKinect ?id) (happiness_ratio ?ratio) (head ?x ?y ?z))
   ?win <- (winner)
   ?face <- (face)
   (test (> ?ratio 0.99))
   =>
   (bind ?x_cal (calib_x ?x ?z))
   (bind ?y_cal (calib_y ?y ?z))
   (modify ?face (ecs 0.80 0.50))
   (modify ?win (id ?id) (point ?x_cal ?y_cal ?z) (lookrule HAPPY))
   (retract ?check) 
   (assert (winner_is_chosen))
   (fun_context "persona felice")
)

(defrule MAIN::lookrule_distance1 "This rule selects the winner as the person 
							   	    who is closer to the robot"
   (declare (salience 90))
   ?check <- (winner_not_chosen)
   (tracking_is ON)
   (subject (idKinect ?id) (head ?x ?y ?z))
   (test (< ?z 0.8))
   ?win <- (winner)
   =>
   (bind ?x_cal (calib_x ?x ?z))
   (bind ?y_cal (calib_y ?y ?z))
   (modify ?win (id ?id) (point ?x_cal ?y_cal ?z) (lookrule DISTANCE1))
   (retract ?check) 
   (assert (winner_is_chosen))
)

(defrule MAIN::lookrule_distance2 "This rule selects the winner as the person 
                               who is closer to the robot"
   (declare (salience 89))
   ?check <- (winner_not_chosen)
   (tracking_is ON)
   (subject (idKinect ?id) (head ?x ?y ?z))
   (test (< ?z 1.4))
   ?win <- (winner)
    =>
   (bind ?x_cal (calib_x ?x ?z))
   (bind ?y_cal (calib_y ?y ?z))
   (modify ?win (id ?id) (point ?x_cal ?y_cal ?z) (lookrule DISTANCE2))
   (retract ?check) 
   (assert (winner_is_chosen))
)

(defrule MAIN::lookrule_distance3 "This rule selects the winner as the person 
                               who is closer to the robot"
   (declare (salience 88))
   ?check <- (winner_not_chosen)
   (tracking_is ON)
   (subject (idKinect ?id) (head ?x ?y ?z))
   (test (< ?z 2))
   ?win <- (winner)
   =>
   (bind ?x_cal (calib_x ?x ?z))
   (bind ?y_cal (calib_y ?y ?z))
   (modify ?win (id ?id) (point ?x_cal ?y_cal ?z) (lookrule DISTANCE3))
   (retract ?check) 
   (assert (winner_is_chosen))
)

(defrule MAIN::lookrule_distance4 "This rule selects the winner as the person 
                               who is closer to the robot"
   (declare (salience 87))
   ?check <- (winner_not_chosen)
   (tracking_is ON)
   (subject (idKinect ?id) (head ?x ?y ?z))
   (test (< ?z 2.5))
   ?win <- (winner)
   =>
   (bind ?x_cal (calib_x ?x ?z))
   (bind ?y_cal (calib_y ?y ?z))
   (modify ?win (id ?id) (point ?x_cal ?y_cal ?z) (lookrule DISTANCE4))
   (retract ?check) 
   (assert (winner_is_chosen))
)

(defrule MAIN::lookrule_distance5 "This rule selects the winner as the person 
                               who is closer to the robot"
   (declare (salience 86))
   ?check <- (winner_not_chosen)
   (tracking_is ON)
   (subject (idKinect ?id) (head ?x ?y ?z))
   (test (< ?z 3.2))
   ?win <- (winner)
   =>
   (bind ?x_cal (calib_x ?x ?z))
   (bind ?y_cal (calib_y ?y ?z))
   (modify ?win (id ?id) (point ?x_cal ?y_cal ?z) (lookrule DISTANCE5))
   (retract ?check) 
   (assert (winner_is_chosen))
)

(defrule MAIN::lookrule_distance6 "This rule selects the winner as the person 
                               who is closer to the robot"
   (declare (salience 85))
   ?check <- (winner_not_chosen)
   (tracking_is ON)
   (subject (idKinect ?id) (head ?x ?y ?z))
   (test (< ?z 4))
   ?win <- (winner)
   =>
   (bind ?x_cal (calib_x ?x ?z))
   (bind ?y_cal (calib_y ?y ?z))
   (modify ?win (id ?id) (point ?x_cal ?y_cal ?z) (lookrule DISTANCE6))
   (retract ?check) 
   (assert (winner_is_chosen))
)


;--------------------------------------------------------EXECUTIVE RULES 

(defrule MAIN::look_at_winner "Finally, this rule makes the robot look at the winner making the expression that is the most suitable to the current social context"
   ?check <- (winner_is_chosen)
   ?face <- (face (ecs ?ev ?ea))
   ?win <- (winner (id ?id) (point ?x ?y ?z) (lookrule ?rulefired))
   =>
   (bind ?x_appr (precision ?x 3))
   (bind ?y_appr (precision ?y 3))
   (bind ?z_appr (precision ?z 3))
   (printout t "LOOK AT ( "?x_appr" , "?y_appr", "?z_appr" ) - ECS (" ?ev " | " ?ea ") - RULE [ "?rulefired" ] - WINNER ( " ?id " )" crlf) 
   (fun_lookat ?id ?x_appr ?y_appr ?z_appr)
   ;(fun_makeexp ?ev ?ea)
   (modify ?win (id 0)(point 0.0 0.0 0.0) (lookrule none))
   (modify ?face (ecs 0.0 0.0))
   (assert (delete subjects))
   (retract ?check)
)


(defrule MAIN::delete_subjects "At the end CLIPS provides to remove all the stored subjects"
   (delete subjects)
   ?s <- (subject)
   =>
   (retract ?s)
)

(defrule MAIN::deleting_is_done "Subjects removal is done!"
   ?del <- (delete subjects)
   (not (subject (idKinect ?)))
   =>
   (retract ?del)
   (bind ?*sm_list* 0)
   (assert (winner_not_chosen))
)

;--------------------------------------------------------EMOTIONAL CONVERSATION RULES

(defrule MAIN::refresh_speaking "Check if the robot has finished to speak"
   ?start <- (speak start)
   ?end <- (speak end)
=>
   (fun_speechstatus 1)
   (retract ?start ?end)
)

(defrule MAIN::check-inizio-parlando
   (speak start)
   =>
   (printout t "STO PARLANDO ............" crlf)
)

(defrule MAIN::check-fine-parlando
   (speak end)
   =>
   (printout t "............ HO DETTO TUTTO." crlf)
)

(defrule MAIN::check-JOY
   ?remove <- (sentence-emotion-is ?what)
   (test (eq ?what JOY))
   =>
   (printout t "sentence-emotion-is " ?what crlf)
   (retract ?remove)
   (fun_makeexp 0.9 0.6)
)

(defrule MAIN::check-ANGER
   ?remove <- (sentence-emotion-is ?what)
   (test (eq ?what ANGER))
   =>
   (printout t "sentence-emotion-is " ?what crlf)
   (retract ?remove)
   (fun_makeexp -0.5 0.6)
)

(defrule MAIN::check-SURPRISE
   ?remove <- (sentence-emotion-is ?what)
   (test (eq ?what SURPRISE))
   =>
   (printout t "sentence-emotion-is " ?what crlf)
   (retract ?remove)
   (fun_makeexp 0.1 0.55)
)

(defrule MAIN::check-FEAR
   ?remove <- (sentence-emotion-is ?what)
   (test (eq ?what FEAR))
   =>
   (printout t "sentence-emotion-is " ?what crlf)
   (retract ?remove)
   (fun_makeexp -0.23 0.73)
)

(defrule MAIN::check-LOVE
   ?remove <- (sentence-emotion-is ?what)
   (test (eq ?what LOVE))
   =>
   (printout t "sentence-emotion-is " ?what crlf)
   (retract ?remove)
   (fun_makeexp 0.7 0.4)
)

(defrule MAIN::check-SADNESS
   ?remove <- (sentence-emotion-is ?what)
   (test (eq ?what SADNESS))
   =>
   (printout t "sentence-emotion-is " ?what crlf)
   (retract ?remove)
   (fun_makeexp -0.5 -0.5)
)

(defrule MAIN::check-NEUTRAL
   ?remove <- (sentence-emotion-is ?what)
   (test (eq ?what NEUTRAL))
   =>
   (printout t "sentence-emotion-is " ?what crlf)
   (retract ?remove)
   (fun_makeexp 0.0 0.0)
)

(defrule MAIN::check-DISGUST
   ?remove <- (sentence-emotion-is ?what)
   (test (eq ?what DISGUST))
   =>
   (printout t "sentence-emotion-is " ?what crlf)
   (retract ?remove)
   (fun_makeexp -0.6 0.35)
)


(defrule MAIN::check-NEUTRAL-POSE
   ?remove <- (sentence-pose-is ?what)
   (test (eq ?what NEUTRAL))
   =>
   (printout t "sentence-pose-is " ?what crlf)
   (retract ?remove)
   (fun_posture "box3" 1)
)


(defrule MAIN::change-word-in-a-sentence
   ?remove <- (sentence-text-is ?what)
   =>
   (retract ?remove)
   (assert (sentence-to-say-is ?what))
)

(defrule MAIN::send-changed-sentence
   ?remove <- (sentence-to-say-is ?what)
   =>
   (fun_sentence_changed ?what)
   (retract ?remove)
)

(defrule MAIN::check-64
   ?remove <- (dai parla-cazzo)
   =>
   (printout t "STO PER PARLARE" crlf)
   (retract ?remove)
   (fun_speech 0)
)
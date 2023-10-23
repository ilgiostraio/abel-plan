(defglobal ?*xfisso* = 0.5) 
(defglobal ?*yfisso* = 0.5) 

(defglobal ?*s* = 0.65) 
(defglobal ?*sm_list* = (create$))

(deftemplate MAIN::subject "These are subject's peculiar 
							characteristics detected by F.A.C.E."
   (slot id (type INTEGER) (default ?DERIVE))
   (slot idKinect (type INTEGER) (default ?DERIVE))
   (slot trackedState (type SYMBOL) (default False) (allowed-symbols False True))
   (multislot name (type LEXEME) (default unknown))
   (slot gender (type SYMBOL) (default unknown) (allowed-symbols Male Female unknown))
   (slot age (type INTEGER) (default ?DERIVE))
   (slot speak_prob)
   (slot gesture (type INTEGER) (default 0))
   (slot uptime (type NUMBER) (default ?DERIVE) (range 0.0 ?VARIABLE))
   (slot angle)
   
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

   
(deftemplate MAIN::winner "This is the winner template; inside you can find ID, point to look and lookrule fired"
   (slot id (type INTEGER) (default 0))
   (multislot point (type NUMBER) (default 0.5 0.5))
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

(deffunction precision (?num ?digits)
(bind ?m (integer (** 10 ?digits)))
(/ (integer (* ?num ?m)) ?m))

(deffunction my-predicate (?fact1 ?fact2)
   (> (fact-slot-value ?fact1 marker) (fact-slot-value ?fact2 marker)))

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
  
(deffacts MAIN::initialization "Just to initialize some useful facts" 
   (winner)
   (winner_not_chosen)
   (face)
   (tracking_is OFF)
   (max_sm)
)

;--------------------------------------------------------STANDARD BEHAVIOR
   
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
 
  
(defrule MAIN::refresh_loneliness "If you were lonely at the beginning you will be it also when nobody is inside!"
   ?trackON <- (tracking_is ON)
   (tracking_is OFF)
=>
   (retract ?trackON)
)

   
(defrule MAIN::boring_loneliness "If the robot doesn't see anyone it's bored and follows the virtual point"
   ?check <- (winner_not_chosen)
   (tracking_is OFF)
   ?surround <- (surroundings (saliency ?x ?y ?) (resolution ?w ?h))
   ?win <- (winner) 
   ?face <- (face (mood ?v ?a))
   =>
   (bind ?nx (/ ?x ?w))
   (bind ?ny (- 1 (/ ?y ?h)))
   (bind ?newv (flatmood ?v))
   (bind ?newa (flatmood ?a))
   (modify ?face (ecs -0.30 -0.45) (mood ?newv ?newa))
   (modify ?win (id 1) (point ?nx ?ny) (lookrule LONELINESS))
   (retract ?surround ?check)
   (assert (winner_is_chosen))
)


(defrule MAIN::lookrule_CrossedArms "This rule selects the winner as the person who is crossing their arms"
   (declare (salience 130))
   ?check <- (winner_not_chosen)
   (tracking_is ON)
   (subject (idKinect ?id) (gesture 4) (head ?x ?y ?))
   ?win <- (winner)
   ?face <- (face (mood ?v ?a))
   =>
   (bind ?newv (moodchange ?v -0.047))
   (bind ?newa (moodchange ?a -0.065))
   (modify ?face (ecs -0.52 -0.67) (mood ?newv ?newa))
   (modify ?win (id ?id) (point ?x ?y) (lookrule Crossed_Arms))
   (retract ?check) 
   (assert (winner_is_chosen))
)

(defrule MAIN::lookrule_intrusive "Ehi! You are too close to me...get out of my intimate space!"
   (declare (salience 125))
   ?check <- (winner_not_chosen)
   (tracking_is ON)
   (subject (idKinect ?id) (head ?x ?y ?z))
   ?win <- (winner)
   ?face <- (face (mood ?v ?a))
   (test (< ?z 1))
   =>
   (bind ?newv (moodchange ?v -0.08))
   (bind ?newa (moodchange ?a 0.03))
   (modify ?face (ecs -0.50 0.60) (mood ?newv ?newa))
   (modify ?win (id ?id) (point ?x ?y) (lookrule INTRUSIVE))
   (retract ?check) 
   (assert (winner_is_chosen))
)

(defrule MAIN::lookrule_RaiseHand "This rule selects the winner as the person who is looking for attention"
   (declare (salience 120))
   ?check <- (winner_not_chosen)
   (tracking_is ON)
   (subject (idKinect ?id) (gesture ?g) (head ?x ?y ?))
   ?win <- (winner)
   ?face <- (face (mood ?v ?a))
   (test (eq ?g 1))
   =>
   (bind ?newv (moodchange ?v 0.19))
   (bind ?newa (moodchange ?a 0.17))
   (modify ?face (ecs 0.21 0.66))
   (modify ?win (id ?id) (point ?x ?y) (lookrule Hand_Raised))
   (retract ?check) 
   (assert (winner_is_chosen))
)

(defrule MAIN::lookrule_Interact "This rule selects the winner as the person who is speaking to the robot"
   (declare (salience 100))
   ?check <- (winner_not_chosen)
   (tracking_is ON)
   (subject (idKinect ?id) (speak_prob ?s) (head ?x ?y ?))
   ?win <- (winner)
   ?face <- (face (mood ?v ?a))
   (test (> ?s 0.95))
   =>
   (bind ?newv (moodchange ?v 0.031))
   (bind ?newa (moodchange ?a 0.021))
   (modify ?face (ecs 0.62 0.20) (mood ?newv ?newa))
   (modify ?win (id ?id) (point ?x ?y) (lookrule Interaction))
   (retract ?check) 
   (assert (winner_is_chosen))
)


(defrule MAIN::lookrule_happyface "This rule selects the winner as the person who is smiling"
   (declare (salience 99))
   ?check <- (winner_not_chosen)
   (tracking_is ON)
   (subject (idKinect ?id) (happiness_ratio ?ratio) (head ?x ?y ?))
   ?win <- (winner)
   ?face <- (face (mood ?v ?a))
   (test (> ?ratio 0.8))
   =>
   (bind ?newv (moodchange ?v 0.2))
   (bind ?newa (moodchange ?a 0.07))
   (modify ?face (ecs 0.20 0.36) (mood ?newv ?newa))
   (modify ?win (id ?id) (point ?x ?y) (lookrule HAPPY))
   (retract ?check) 
   (assert (winner_is_chosen))
)

;-----------------------------------------------------------------DISTANCE rules for tracking

(defrule MAIN::lookrule_distance1 "This rule selects the winner as the person 
							   	    who is closer to the robot"
   (declare (salience 89))
   ?check <- (winner_not_chosen)
   (tracking_is ON)
   (subject (idKinect ?id) (head ?x ?y ?z))
   ?win <- (winner)
   ?face <- (face)
   (test (and (< ?z 1.4) (>= ?z 1)))
   =>
   (modify ?face (ecs 0.0 0.0))
   (modify ?win (id ?id) (point ?x ?y) (lookrule DISTANCE_1))
   (retract ?check) 
   (assert (winner_is_chosen))
)


(defrule MAIN::lookrule_distance2 "This rule selects the winner as the person 
							   	    who is closer to the robot"
   (declare (salience 88))
   ?check <- (winner_not_chosen)
   (tracking_is ON)
   (subject (idKinect ?id) (head ?x ?y ?z))
   ?win <- (winner)
   ?face <- (face)
   (test (and (< ?z 1.6) (>= ?z 1.4)))
   =>
   (modify ?face (ecs 0.0 0.0))
   (modify ?win (id ?id) (point ?x ?y) (lookrule DISTANCE_2))
   (retract ?check) 
   (assert (winner_is_chosen))
)



(defrule MAIN::lookrule_distance3 "This rule selects the winner as the person 
							   	    who is closer to the robot"
   (declare (salience 87))
   ?check <- (winner_not_chosen)
   (tracking_is ON)
   (subject (idKinect ?id) (head ?x ?y ?z))
   ?win <- (winner)
   ?face <- (face)
   (test (and (< ?z 1.8) (>= ?z 1.6)))
   =>
   (modify ?face (ecs 0.0 0.0))
   (modify ?win (id ?id) (point ?x ?y) (lookrule DISTANCE_3))
   (retract ?check) 
   (assert (winner_is_chosen))
)


(defrule MAIN::lookrule_distance4 "This rule selects the winner as the person 
							   	    who is closer to the robot"
   (declare (salience 86))
   ?check <- (winner_not_chosen)
   (tracking_is ON)
   (subject (idKinect ?id) (head ?x ?y ?z))
   ?win <- (winner)
   ?face <- (face)
   (test (and (< ?z 2) (>= ?z 1.8)))
   =>
   (modify ?face (ecs 0.0 0.0))
   (modify ?win (id ?id) (point ?x ?y) (lookrule DISTANCE_4))
   (retract ?check) 
   (assert (winner_is_chosen))
)


(defrule MAIN::lookrule_distance5 "This rule selects the winner as the person 
							   	    who is closer to the robot"
   (declare (salience 85))
   ?check <- (winner_not_chosen)
   (tracking_is ON)
   (subject (idKinect ?id) (head ?x ?y ?z))
   ?win <- (winner)
   ?face <- (face)
   (test (and (< ?z 2.5) (>= ?z 2)))
   =>
   (modify ?face (ecs 0.0 0.0))
   (modify ?win (id ?id) (point ?x ?y) (lookrule DISTANCE_5))
   (retract ?check) 
   (assert (winner_is_chosen))
)

(defrule MAIN::lookrule_distance6 "This rule selects the winner as the person 
							   	    who is closer to the robot"
   (declare (salience 84))
   ?check <- (winner_not_chosen)
   (tracking_is ON)
   (subject (idKinect ?id) (head ?x ?y ?z))
   ?win <- (winner)
   ?face <- (face)
   (test (and (< ?z 3) (>= ?z 2.5)))
   =>
   (modify ?face (ecs 0.0 0.0))
   (modify ?win (id ?id) (point ?x ?y) (lookrule DISTANCE_6))
   (retract ?check) 
   (assert (winner_is_chosen))
)


(defrule MAIN::lookrule_distance7 "This rule selects the winner as the person 
							   	    who is closer to the robot"
   (declare (salience 83))
   ?check <- (winner_not_chosen)
   (tracking_is ON)
   (subject (idKinect ?id) (head ?x ?y ?z))
   ?win <- (winner)
   ?face <- (face)
   (test (and (< ?z 4) (>= ?z 3)))
   =>
   (modify ?face (ecs 0.0 0.0))
   (modify ?win (id ?id) (point ?x ?y) (lookrule DISTANCE_7))
   (retract ?check) 
   (assert (winner_is_chosen))
)


;--------------------------------------------------------EXECUTIVE RULE

(defrule MAIN::look_at_winner "Finally, this rule makes the robot look at the winner making the expression that is the most suitable to the current social context"
   ?check <- (winner_is_chosen)
   ?face <- (face (ecs ?ev ?ea) (mood ?mv ?ma))
   ?win <- (winner (id ?id) (point ?x ?y) (lookrule ?rulefired))
   (max_sm (id ?idSM))
   =>
   (bind ?new_ev (+ ?ev (* ?mv 0.1)))
   (bind ?new_ea (+ ?ea (* ?ma 0.1)))
   (bind ?x_appr (precision ?x 2))
   (bind ?y_appr (precision ?y 2))
   (printout t "[" ?id " |" ?x_appr "|" ?y_appr "] ECS ( 0.6 | 0.5 )  [ "?rulefired" ]"  crlf)
   (fun_lookat ?id ?x_appr ?y_appr)
   (fun_makeexp 0.6 0.5)
   (modify ?win (id 0) (point 0.0 0.0) (lookrule none))
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

;---------------------------------------------------SOMATIC MARKER MANAGEMENT SIMPLE

(defrule assign_sm
   (winner_is_chosen)
   (winner (id ?id))
   (not (sm (id ?id)))
   (face (ecs ?ev ?ea)(mood ?v ?a))
   (test (> (sqrt (+ (* ?v ?v) (* ?a ?a))) ?*s*))
   =>
   (bind ?marker (* ?v 100))
   (assert (sm (id ?id) (marker ?marker) (bp ?ev ?ea)))
   (printout t "NEW MARKER FOR "?id" -> "?marker" ! " crlf)
)



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
   (slot LookingAway (type LEXEME) (default Unknown) (allowed-symbols Yes No Unknown Maybe Analysed))
   (slot WearingGlasses (type LEXEME) (default Unknown) (allowed-symbols Yes No Unknown Maybe))
   (slot MouthOpen (type LEXEME) (default Unknown) (allowed-symbols Yes No Unknown Maybe))
   (slot MouthMoved (type LEXEME) (default Unknown) (allowed-symbols Yes No Unknown Maybe))
   (slot Engaged (type LEXEME) (default Unknown) (allowed-symbols Yes No Unknown Maybe Analysed))
) 

   
(deftemplate MAIN::surroundings "These are surrounding's peculiar 
characteristics detected by F.A.C.E."
   (slot soundEstimatedX (type NUMBER) (default ?DERIVE))
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
   (slot soundAverageNorm (type NUMBER) (default 0))
   (slot soundDecibelNorm (type NUMBER) (default 0))
)

   
(deftemplate MAIN::winner "This is the winner template, inside you can find ID, point to look and lookrule fired"
   (slot id (type INTEGER) (default 0))
   (multislot point (type NUMBER) (default 0.5 0.5))
   (slot lookrule (type LEXEME) (default none))
)


(deftemplate MAIN::face "This is the robot emotional state template, containing its current mood and expression"
   (multislot mood (type NUMBER) (default 0.0 0.0))
   (multislot ecs (type NUMBER) (default 0.0 0.0))
)


(deftemplate MAIN::ecoexp "This is the economics experiment template"
   (slot id_player (type INTEGER) (default 0))
   (slot exp_mode (type LEXEME) (allowed-symbols empty promising unknown) (default unknown))
   (slot outcome (type LEXEME) (allowed-symbols roll dontroll unknown) (default unknown))
   (slot event (type LEXEME) (default unknown))
   (slot event_id (type INTEGER))
)


(deffunction moodchange "This is the function for changing the mood values without going out from the interval (-1.0 | 1.0)" 
(?m ?n)
(bind ?newm (+ ?n ?m))
(if (and (> ?newm -1.0) (< ?newm 0)) then (return ?newm))
(if (= ?newm 0) then (return ?newm))
(if (<= ?newm -1.0) then (return -1.0))
(if (and (> ?newm 0) (< ?newm 1.0)) then (return ?newm))
(if (>= ?newm 1.0) then (return 1.0))
)

(deffunction flatmood "This is the function for changing the mood values bringing them to (0.0 | 0.0)" 
(?m)
(if (and (> ?m 0.0) (<= ?m 0.015)) then (return 0.0))
(if (= ?m 0.0) then (return 0.0))
(if (and (< ?m 0.0) (>= ?m -0.015)) then (return 0.0))
(if (< ?m -0.015) then 
(bind ?newm (+ ?m 0.0015))
(return ?newm))
(if (> ?m 0.015) then 
(bind ?newm (- ?m 0.0015))
(return ?newm))
)

  
(deffacts MAIN::initialization "Just to initialize some useful facts" 
   (winner)
   (winner_not_chosen)
   (face)
   (tracking_is OFF)
   (ecoexp)
   (experiment not_started)
   (speak_timer 0)
)

   
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
 
  
(defrule MAIN::refresh_loneliness "If you were lonely at the beginning you will be lonely also when nobody is inside!"
   ?trackON <- (tracking_is ON)
   (tracking_is OFF)
=>
   (retract ?trackON)
)

(defrule MAIN::refresh_speaking "Check if the robot has finished to speak"
   ?start <- (speak start)
   ?end <- (speak end)
=>
   (retract ?start ?end)
)

   
(defrule MAIN::boring_loneliness "If the robot doesn't see anyone it's bored and follows the virtual point"
   ?check <- (winner_not_chosen)
   (tracking_is OFF)
   ?surround <- (surroundings (saliency ?x ?y ?) (resolution ?w ?h))
   ?win <- (winner) 
   ?face <- (face (mood ?v ?a))
   =>
   (bind ?nx (/ ?x ?w))
   (bind ?ny (/ ?y ?h))
   (bind ?newv (flatmood ?v))
   (bind ?newa (flatmood ?a))
   (modify ?face (ecs -0.58 -0.24) (mood ?newv ?newa))
   (modify ?win (id 1) (point ?nx ?ny) (lookrule LONELINESS))
   (retract ?surround ?check)
   (assert (winner_is_chosen))
)

(defrule MAIN::delete_clock_without_subjects 
   (tracking_is OFF)
   ?t <- (time $?)
   =>
   (retract ?t)
)


(defrule MAIN::lookrule_RaiseHand "This rule selects the winner as the person who is looking for attention"
(declare (salience 200))
   ?exp <- (experiment not_started)
   ?t <- (speak_timer 0)
   ?check <- (winner_not_chosen)
   ?ecoexp <- (ecoexp (id_player ?id_player))
   (tracking_is ON)
   (subject (idKinect ?id) (gesture ?g) (head ?x ?y ?) (angle ?a))
   ?win <- (winner)
   ?face <- (face)
   (test (and (> ?a -19)(< ?a 19)))
   (test (eq ?g 1))
   =>
   (modify ?win (id ?id) (point ?x ?y) (lookrule RAISE_HAND))
   (modify ?ecoexp (id_player (+ ?id_player 1)) (event EXPERIMENT_BEGINS))
   (retract ?check ?exp ?t) 
   (assert (speak_timer 1))
   (assert (wait_for sentence_0))
   (assert (winner_is_chosen))
   (assert (select exp_mode))
   (assert (session ongoing))
)

(defrule MAIN::exp_mode_promising "this rule selects the mode of the experiment according to the id_player number"
   ?s <- (select exp_mode)
   ?ecoexp <- (ecoexp (id_player ?id))
   (test (oddp ?id))
   =>
   (modify ?ecoexp (exp_mode promising))
   (retract ?s)
)

(defrule MAIN::exp_mode_empty "this rule selects the mode of the experiment according to the id_player number"
   ?s <- (select exp_mode)
   ?ecoexp <- (ecoexp (id_player ?id))
   (test (evenp ?id))
   =>
   (modify ?ecoexp (exp_mode empty))
   (retract ?s)
)


(defrule MAIN::lookrule_intrusive "This rule selects the winner as the person 
     who is too close to the robot, who is bothered"
   (declare (salience 180))
   (not (speak ?))
   ?check <- (winner_not_chosen)
   (tracking_is ON)
   (subject (idKinect ?id) (head ?x ?y ?z))
   ?win <- (winner)
   ?face <- (face (mood ?v ?a))
   (test (and (< ?z 1) (>= ?z 0.5)))
   =>
   (bind ?newv (moodchange ?v -0.24))
   (bind ?newa (moodchange ?a 0.35))
   (modify ?face (ecs -0.51 0.58) (mood ?newv ?newa))
   (modify ?win (id ?id) (point ?x ?y) (lookrule INTRUSIVE))
   (retract ?check) 
   (assert (winner_is_chosen))
)

(defrule MAIN::moodrule_empathy "This rule makes the robot happy if the subject is smiling"
   (declare (salience 150))
   (not (speak ?))
   ?check <- (winner_not_chosen)
   (tracking_is ON)
   ?sub <- (subject (idKinect ?id) (head ?x ?y ?) (happiness_ratio ?happy))
   ?win <- (winner)
   ?face <- (face (mood ?v ?a))
   (test (> ?happy 0.7))
   =>
   (bind ?newv (moodchange ?v (/ ?happy 2200)))
   (bind ?newa (moodchange ?a (/ ?happy 2400)))
   (modify ?face (mood ?newv ?newa))
   (modify ?win (id ?id) (point ?x ?y) (lookrule EMPATHY))
   (retract ?check) 
   (assert (winner_is_chosen))
)

(defrule MAIN::moodrule_angry "This rule makes the robot angry if the subject is having an expression of anger"
   (declare (salience 150))
   (not (speak ?))
   ?check <- (winner_not_chosen)
   (tracking_is ON)
   ?sub <- (subject (idKinect ?id) (head ?x ?y ?) (anger_ratio ?anger))
   ?win <- (winner)
   ?face <- (face (mood ?v ?a))
   (test (> ?anger 0.5))
   =>
   (bind ?newv (moodchange ?v (/ ?anger -1900)))
   (bind ?newa (moodchange ?a (/ ?anger 1800)))
   (modify ?face (mood ?newv ?newa))
   (modify ?win (id ?id) (point ?x ?y) (lookrule ANGRY))
   (retract ?check) 
   (assert (winner_is_chosen))
)



(defrule MAIN::lookrule_CrossedArms "This rule selects the winner as the person who is crossing their arms"
   (declare (salience 140))
   (not (speak ?))
   ?check <- (winner_not_chosen)
   (tracking_is ON)
   (subject (idKinect ?id) (gesture 4) (head ?x ?y ?))
   ?win <- (winner)
   ?face <- (face (mood ?v ?a))
   =>
   (bind ?newv (moodchange ?v -0.03))
   (bind ?newa (moodchange ?a -0.028))
   (modify ?face (mood ?newv ?newa))
   (modify ?win (id ?id) (point ?x ?y) (lookrule Crossed_Arms))
   (retract ?check) 
   (assert (winner_is_chosen))
)


(defrule MAIN::moodrule_not_engaged "This rule makes the robot sad if the subject is not Engaged"
   (declare (salience 100))
   (not (speak ?))
   ?check <- (winner_not_chosen)
   (tracking_is ON)
   ?sub <- (subject (idKinect ?id) (head ?x ?y ?) (Engaged ?eng) (LookingAway ?look))
   ?win <- (winner)
   ?face <- (face (mood ?v ?a))
   (test (and (eq ?eng No) (eq ?look Yes)))
   =>
   (bind ?newv (moodchange ?v -0.0025))
   (bind ?newa (moodchange ?a -0.008))
   (modify ?face (mood ?newv ?newa))
   (modify ?win (id ?id) (point ?x ?y) (lookrule NOT_ENGAGED))
   (modify ?sub (Engaged Analysed) (LookingAway Analysed))
   (retract ?check) 
   (assert (winner_is_chosen))
)

(defrule MAIN::moodrule_engaged "This rule makes the robot happy if the subject is Engaged"
   (declare (salience 100))
   (not (speak ?))
   ?check <- (winner_not_chosen)
   (tracking_is ON)
   ?sub <- (subject (idKinect ?id) (head ?x ?y ?) (Engaged ?eng) (LookingAway ?look))
   ?win <- (winner)
   ?face <- (face (mood ?v ?a))
   (test (and (eq ?eng Yes) (eq ?look No)))
   =>
   (bind ?newv (moodchange ?v 0.03))
   (bind ?newa (moodchange ?a -0.005))
   (modify ?face (mood ?newv ?newa))
   (modify ?win (id ?id) (point ?x ?y) (lookrule ENGAGED))
   (modify ?sub (Engaged Analysed) (LookingAway Analysed))
   (retract ?check) 
   (assert (winner_is_chosen))
)



(defrule MAIN::lookrule_eyecontact "This rule selects the winner as the person 
     who is closer to the robot"
   (declare (salience 50))
   ?check <- (winner_not_chosen)
   (tracking_is ON)
   (subject (idKinect ?id) (head ?x ?y ?z))
   ?win <- (winner)
   ?face <- (face (mood ?v ?a))
   (test (and (< ?z 2.5) (>= ?z 1)))
   =>
   (bind ?newv (flatmood ?v))
   (bind ?newa (flatmood ?a))
   (modify ?face (mood ?newv ?newa))
   (modify ?win (id ?id) (point ?x ?y) (lookrule EYE_CONTACT))
   (retract ?check) 
   (assert (winner_is_chosen))
)   


(defrule MAIN::look_at_winner "Finally, this rule makes the robot look at the winner making the expression that is the most suitable to the current social context"
   ?check <- (winner_is_chosen)
   ?face <- (face (ecs ?ev ?ea) (mood ?mv ?ma))
   ?win <- (winner (id ?id) (point ?x ?y) (lookrule ?rulefired))
   ?ecoexp <- (ecoexp (id_player ?id_player)(exp_mode ?exp_mode)(outcome ?outcome)(event ?event)(event_id ?e_id))
   (speak_timer ?t)
   ?clock <- (time ?clock1 ?clock2) 
   (test (and (neq ?id 0) (neq ?rulefired none)))
   =>
   (printout t "["?clock2"] " ?id_player " | " ?rulefired ", Mode("?exp_mode"), E("?ev"|"?ea"), M("?mv"|"?ma"), Out("?outcome"), t(" ?t "), Event("?event"|"?e_id")" crlf)  
   ;(printout mydata ?clock1 "," ?clock2 "," ?id_player "," ?exp_mode "," ?rulefired "," ?mv "," ?ma "," ?outcome "," ?event "," ?e_id crlf)  
   ;(fun_lookat ?id ?x ?y)
   ;(fun_makeexp ?ev ?ea)
   ;(fun_mood ?mv ?ma)
   (modify ?win (id 0) (point 0.09 0.07) (lookrule none))
   (modify ?face (ecs 0.09 0.07))
   (assert (delete subjects))
   (assert (decide roll))
   (assert (update speak_timer))
   (retract ?check ?clock)
)


(defrule MAIN::delete_subjects "At the end CLIPS provides to remove all the stored subjects"
   (delete subjects)
   ?s <- (subject)
   =>
   (retract ?s)
)

(defrule MAIN::delete_clock
(declare (salience 1000))
   (delete subjects)
   ?t <- (time $?)
   =>
   (retract ?t)
)

(defrule MAIN::deleting_is_done "Subjects removal is done!"
   ?del <- (delete subjects)
   (not (subject (idKinect ?)))
   =>
   (retract ?del)
   (assert (winner_not_chosen))
)


(defrule counting_time "This rule bring the time counter to zero"
   ?update <- (update speak_timer)
   ?speak_timer <- (speak_timer ?t)
   (test (> ?t 0))
   =>
   (bind ?new_t (- ?t 1))
   (retract ?speak_timer ?update)
   (assert (speak_timer ?new_t))
)

(defrule stop_updating_speak_timer 
   ?update <- (update speak_timer)
   (speak_timer 0)
   =>
   (retract ?update)
)

(defrule say_the_welcome_sentence
   (not (speak ?))
   ?speak_timer <- (speak_timer 0)
   ?wait <- (wait_for sentence_0)
   ?ecoexp <- (ecoexp)
   =>
   (bind ?n 0)
   (fun_speech ?n)
   (retract ?wait ?speak_timer)
   (assert (speak_timer 8))
   (assert (wait_for sentence_1))
   (modify ?ecoexp (event WELCOME_sentence))
)

(defrule say_the_empty_sentence
   (not (speak ?))
   ?speak_timer <- (speak_timer 0)
   ?wait <- (wait_for sentence_1)
   ?ecoexp <- (ecoexp (exp_mode empty))
   =>
   (retract ?wait ?speak_timer)
   (bind ?n (random 1 4))
   (fun_speech ?n)
   (if (or (eq ?n 1) (eq ?n 2)) then (bind ?timer 55))
   (if (eq ?n 3) then (bind ?timer 7))
   (if (eq ?n 4) then (bind ?timer 17))
   (assert (speak_timer ?timer))
   (assert (wait_for sentence_2))
   (modify ?ecoexp (event EMPTY_sentence) (event_id ?n))
)

(defrule say_the_promising_sentence
   (not (speak ?))
   ?speak_timer <- (speak_timer 0)
   ?wait <- (wait_for sentence_1)
   ?ecoexp <- (ecoexp (exp_mode promising))
   =>
   (retract ?wait ?speak_timer)
   (bind ?n (random 5 8))
   (fun_speech ?n)
   (if (eq ?n 6) then (bind ?timer 36))
   (if (eq ?n 5) then (bind ?timer 50))
   (if (eq ?n 7) then (bind ?timer 7))
   (if (eq ?n 8) then (bind ?timer 21))
   (assert (speak_timer ?timer))
   (assert (wait_for sentence_2))
   (modify ?ecoexp (event PROMISING_sentence) (event_id ?n))
)

(defrule say_the_do_it_sentence
   (not (speak ?))
   (speak_timer 0)
   ?wait <- (wait_for sentence_2)
   ?ecoexp <- (ecoexp)
   =>
   (retract ?wait)
   (bind ?n 9)
   (fun_speech ?n)
   (modify ?ecoexp (event DO_IT_sentence))
)

(defrule decide_to_roll "this rule makes the robot deciding to roll the dice or not according to its mood"
   ?d <- (decide roll)
   (face (mood ?v ?))
   ?ecoexp <- (ecoexp)
   (test (> ?v 0))
   =>
   (modify ?ecoexp (outcome roll))
   (retract ?d)
)

(defrule decide_not_to_roll "this rule makes the robot deciding to roll the dice or not according to its mood"
   ?d <- (decide roll)
   (face (mood ?v ?))
   ?ecoexp <- (ecoexp)
   (test (< ?v 0))
   =>
   (modify ?ecoexp (outcome dontroll))
   (retract ?d)
)

(defrule decide_random "this rule makes the robot deciding to roll the dice or not according to its mood"
   ?d <- (decide roll)
   (face (mood ?v ?))
   ?ecoexp <- (ecoexp)
   (test (= ?v 0))
   =>
   (bind ?dice (random 0 1))
   (if (= ?dice 0) then
   (modify ?ecoexp (outcome dontroll)))
   (if (= ?dice 1) then
   (modify ?ecoexp (outcome roll)))
   (retract ?d)
)

(defrule session_end_dontroll "this rule understands when a session is finished and printout the outcome"
(declare (salience -200))
   ?end <- (session ongoing)
   (tracking_is OFF)
   ?ecoexp <- (ecoexp (outcome dontroll))
   ?face <- (face)
   =>
   (modify ?ecoexp (outcome unknown) (event SESSION_COMPLETED) (event_id 0))
   (modify ?face (mood 0.0 0.0))
   (assert (experiment not_started))
   (retract ?end)
)

(defrule session_end_roll "this rule understands when a session is finished and printout the outcome"
(declare (salience -200))
   ?end <- (session ongoing)
   ?face <- (face)
   (tracking_is OFF)
   ?ecoexp <- (ecoexp (outcome roll))
   =>
   (bind ?dice (random 1 6))
   (modify ?ecoexp (outcome unknown) (event SESSION_COMPLETED) (event_id ?dice))
   (modify ?face (mood 0.0 0.0))
   (assert (experiment not_started))
   (retract ?end)
)


(deftemplate iclips-module (slot name))

(deffunction MAIN::require (?n) (return (if (not (any-factp ((?module iclips-module)) (eq ?module:name ?n))) then (load-module ?n) else TRUE)))
(deffunction MAIN::reject (?n) (return (if (not (any-factp ((?module iclips-module)) (eq ?module:name ?n))) then (unload-module ?n) else TRUE)))

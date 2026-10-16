@include "Source.ks" .

# Fetch all subclasses of the event type.
events = code | extract { 
        ?cls arch:inherits <clr:/Common/FfAdmin.Common/Event> ;
             a arch:Class .
    }

# There should be event types
verify events is not empty message "No event types found!"

# Cross references these types with wiki pages
events_with_wiki = events | enhance { ?cls rdf:type arch:Class } 
                            with (wiki)  { ?page s:about ?cls }
                            where { ?page rdf:type s:DigitalDocument }

# Every subclass of Event should have a wiki page
verify events_with_wiki each { ?cls rdf:type arch:Class } has { ?page s:about ?cls } 
    message "Event {?cls} has no Wiki page!"


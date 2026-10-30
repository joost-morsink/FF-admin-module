@include "Source.ks" .
@include "Functions.ks" .

# Fetch all subclasses of the event type.
events = code | fetch_events()

# There should be event types
verify events is not empty message "No event types found!"

# Cross references these types with wiki pages
events_with_wiki = events | fetch_related_documentation(wiki)

# Every subclass of Event should have a wiki page
verify events_with_wiki 
    each { ?cls a arch:Class . optional {?cls rdfs:label ?label } }
    has { ?page s:about ?cls }
    message "Event {?label} has no Wiki page! ({?cls})"

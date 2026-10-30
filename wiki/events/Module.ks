@prefix arch: <http://bit-wizards.nl/arch/> .
@prefix table: <http://bit-wizards.nl/giki/table/> .
@prefix eventtype: <clr:/Common/FfAdmin.Common/EventType/> .
@prefix common: <clr:/Common/FfAdmin.Common/> .
@prefix ff: <http://giveforgood.world/ff/> .

@fun check_event this(?enum, ?class) {
    enum = this | extract { ?enum a arch:Field ;
        arch:fieldType common:EventType . }
    class = this | extract { ?class a arch:Class ;
        arch:inherits common:Event}
    verify enum is not empty message "Event type does not exist."
    verify class is not empty message "Event class does not exist."
    check_event = {}
} .

@fun check_event_properties this(?cls, assumptions) {
    props = this 
        | extract { 
            ?cls arch:hasProperty ?p. 
            optional { ?p rdfs:label ?lbl . } }
            where { not { ?p arch:modifier arch:Static } }

    verify props each { [arch:hasProperty ?p] . optional { ?p rdfs:label ?lbl . } } has { from(assumptions) { [arch:hasProperty ?p ] } } message "Property `{?lbl|?p}` is not documented"
    verify assumptions each { [arch:hasProperty ?p] } has { from(props) { [arch:hasProperty ?p ] } } message "{?p} does not exist"
    
    check_event_properties = {}

} .

@fun assert_default_descriptions this() {
    assert_default_descriptions = this | assert {
        <clr:/System.Private.CoreLib/System/String> rdfs:label "String" .
        <clr:/System.Private.CoreLib/System/Decimal> rdfs:label "Decimal".
        <clr:/System.Private.CoreLib/System/DateTimeOffset> rdfs:label "DateTime (ISO-8601)".
    }
} .

@fun event_table this(?cls) {
    tabledef = {  _:t a table:Table ;
        table:order [table:property arch:definitionOrder];
        table:columns (
            [table:property rdfs:label ;
            rdfs:label "Field" ;
            table:format table:Markdown ;
            table:template "`{value}`" ] 
            [table:property ff:cstype ;
            rdfs:label "Type" ] 
            [table:property ff:description ;
            rdfs:label "Description" ] 
            [table:property ff:value ;
            rdfs:label "Value" ;
            table:format table:Markdown ] 
        ) .
    }

    tsprop = this
        | extract {
            <clr:/Common/FfAdmin.Common/Event/Timestamp> a arch:Property ;
                rdfs:label ?lbl ;
                arch:propertyType ?pt .
        }
        | infer { ?cls arch:hasProperty ?p }
        where { ?p a arch:Property }

    event_table = this
        | assert_default_descriptions()
        | merge tsprop
        | extract { ?cls arch:hasProperty ?p.
            ?p a arch:Property ;
            rdfs:label ?pname .
            optional { ?p arch:modifier ?s . } .
            optional { ?p arch:propertyType ?pt . ?pt rdfs:label ?ptl . } .
            optional { ?p arch:definitionOrder ?defOrder . }
        }
            where { not { ?p arch:modifier arch:Static } }
        | retract { <clr:/Common/FfAdmin.Common/Event/Timestamp> arch:definitionOrder ?o }
        | assert { <clr:/Common/FfAdmin.Common/Event/Timestamp> arch:definitionOrder -1 }
        | merge tabledef
        | infer { ?t table:rows ?p. ?p ff:cstype ?pt. }
           where { ?p a arch:Property. ?p arch:propertyType/rdfs:label ?pt . ?t a table:Table . }
        | assert {
            <clr:/Common/FfAdmin.Common/Event/Timestamp> ff:description "The timestamp of the event" .
        }
} .



@fun documented_event_table this(?cls, docs) {
    assumptions = docs | infer { ?cls arch:hasProperty ?p } where { ?p ff:description ?d }
    _ = this | check_event_properties(?cls, assumptions)
    documented_event_table = this
        | event_table(?cls)
        | merge docs
} .


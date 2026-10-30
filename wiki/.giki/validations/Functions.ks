@prefix ff: <http://val.giveforgood.world/> .
@prefix arch: <http://bit-wizards.nl/arch/> .
@prefix s: <http://schema.org/> .
@fun mark_obsolete this () {
    mark_obsolete = this 
        |   infer { ?c a ff:Obsolete } 
           where { ?c arch:hasAttributeApplication [arch:attributeType <clr:/System.Private.CoreLib/System/ObsoleteAttribute> ] . }
} .
@fun fetch_related_attributes this (code) {
    fetch_related_attributes = this 
        |   enhance { ?c a arch:Class}
            with(code) { 
                ?c  a arch:Class ;
                    arch:hasAttributeApplication ?a .
                ?a arch:attributeType ?at  } 
} .
@fun fetch_models this () {
    fetch_models = this
    |   extract { 
            ?c  arch:inAssembly <clr:/Calculator> ;
                a arch:Class;
                arch:implements 
                [   a arch:GenericType;
                    arch:genericDefinition <clr:/Calculator.Core/FfAdmin.Calculator/IModel%601> ;
                    arch:hasTypeArgument [ arch:typeArgumentType ?c ]
                ] .
            optional { ?c rdfs:label ?label } 
        }
} .
@fun fetch_events this () {
    fetch_events = this
        |   extract { 
                ?cls arch:inherits <clr:/Common/FfAdmin.Common/Event> ;
                    a arch:Class . 
                optional { ?cls rdfs:label ?label } 
            }
} .

@fun fetch_related_documentation this (wiki) {
    fetch_related_documentation = this
        |   enhance { ?cls a arch:Class } 
            with (wiki) { 
                ?page s:about ?cls . 
                ?page a s:DigitalDocument }
} .
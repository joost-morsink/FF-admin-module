@include "Source.ks" .
@prefix tmp: <http://example.org/tmp#> .


# Fetch all classes that implement the IModel interface of itself as a generic type.
models = code 
    | extract { ?c
                arch:inAssembly <clr:/Calculator> ;
                a arch:Class;
                rdfs:label ?label ;
                arch:implements 
                [   a arch:GenericType;
                    arch:genericDefinition <clr:/Calculator.Core/FfAdmin.Calculator/IModel%601> ;
                    arch:hasTypeArgument [ arch:typeArgumentType ?c ]
                ] .
            } 
    | infer { ?c a tmp:obsolete } # Exclude obsolete classes
            where 
            { from (code) { 
                ?c  arch:inAssembly <clr:/Calculator> ;
                    a arch:Class ;
                    arch:hasAttributeApplication [arch:attributeType <clr:/System.Private.CoreLib/System/ObsoleteAttribute>] . 
                } 
            }

verify models is not empty message "No models found!"

# Fetch all associated wiki pages to the models.
models_with_wiki = models | enhance { ?cls rdf:type arch:Class } 
                            with (wiki)  { ?page s:about ?cls }
                            where { ?page rdf:type s:DigitalDocument }

# Verify that either a model has a wiki page or it is declared obsolete.
verify models_with_wiki # | merge exclusions
    each { ?cls rdf:type arch:Class . ?cls rdfs:label ?label } 
    has { { ?page s:about ?cls } or { ?cls a tmp:obsolete } }
    message "Model {?label} has no Wiki page! ({?cls})"
            
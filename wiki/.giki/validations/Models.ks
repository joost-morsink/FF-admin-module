@prefix arch: <http://bit-wizards.nl/arch/> .
@prefix s: <http://schema.org/> .
@prefix ff: <http://val.giveforgood.world/> .

# Fetch all classes that implement the IModel interface and mark obsolete models.
models = code 
    | fetch_models()
    | fetch_related_attributes(code)
    | mark_obsolete()

verify models | extract { ?c a arch:Class } is not empty message "No models found!"

# Fetch all associated wiki pages to the models.
models_with_wiki = models | fetch_related_documentation(wiki)

# Verify that either a model has a wiki page or it is declared obsolete.
verify models_with_wiki # | merge exclusions
    each { ?cls a arch:Class . optional { ?cls rdfs:label ?label } } 
    has { { ?page s:about ?cls } or { ?cls a ff:Obsolete } }
    message "Model {?label} has no Wiki page! ({?cls})"
            
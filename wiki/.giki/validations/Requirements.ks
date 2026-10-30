@prefix archimate: <http://bit-wizards.nl/arch/archimate/> .

# Get all documented requirements
requirements = wiki | extract { ?req rdf:type archimate:Requirement}

# Get all documented capabilities
capabilities = wiki | extract { ?cap a archimate:Capability }

# Get all realizations of requirements by capabilities
realizations = requirements | merge capabilities
    | enhance { ?req a archimate:Requirement. ?cap a archimate:Capability }
    with(wiki) { ?cap archimate:realizes ?req }

# Verify that all requirements are realized by capabilities
verify requirements is not empty message "No requirements found!"
verify capabilities is not empty message "No capabilities found!"

verify realizations
    each { ?req a archimate:Requirement } 
    has { ?cap a archimate:Capability . ?cap archimate:realizes ?req }
    message "{?req} is not realized by any capability."

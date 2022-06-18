local i18n = {
    get = function(k)
        return k
    end,
}

local en_US = {}

function en_US.you()
    return i18n.get "chara.you"
end

function en_US.name(obj, ignore_sight)
    -- >>>>>>>> shade2/init.hsp:4082 	#defcfunc name int tc ..
    if type(obj) == "table" then
        if obj.is_player then
            return en_US.you()
        end

        if not obj.is_visible and not ignore_sight then
            return i18n.get "chara.something"
        end

        local name = obj.name or i18n.get "chara.something"
        local first = name:sub(1, 1)

        -- HACK should be a property instead, like `has_own_name = true/"random"`
        if first == "\"" or first == "<" or first == "{" then
            return name
        elseif not obj.has_own_name then
            return "the " .. name
        else
            return name
        end
    end
    -- <<<<<<<< shade2/init.hsp:4090 	return cnName(tc) ..

    return i18n.get "chara.something"
end

function en_US.basename(obj)
    if type(obj) == "table" then
        return obj.basename or obj.name or i18n.get "chara.something"
    end

    return i18n.get "chara.something"
end

function en_US.itemname(obj)
    if type(obj) == "table" then
        return obj.name or i18n.get "chara.something"
    end

    return i18n.get "chara.something"
end

en_US.itembasename = en_US.basename

function en_US.ordinal(n)
    if n % 10 == 1 and n ~= 11 then
        return tostring(n) .. "st"
    elseif n % 10 == 2 and n ~= 12 then
        return tostring(n) .. "nd"
    elseif n % 10 == 3 and n ~= 13 then
        return tostring(n) .. "rd"
    else
        return tostring(n) .. "th"
    end
end

function en_US.s(obj, need_e)
    if type(obj) == "number" then
        if obj > 1 then
            return ""
        else
            return "s"
        end
    elseif type(obj) == "table" then
        if obj.amount then
            if obj.amount > 1 then
                return ""
            else
                return "s"
            end
        end

        if obj.is_player then
            return ""
        elseif need_e then
            return "es"
        else
            return "s"
        end
    end

    return "s"
end

function en_US.is(obj)
    if type(obj) == "number" then
        if obj > 1 then
            return "are"
        end
    elseif type(obj) == "table" then
        if obj.amount then
            if obj.amount > 1 then
                return ""
            else
                return "is"
            end
        end

        if obj.is_player then
            return "are"
        end
    end

    return "is"
end

function en_US.have(obj)
    if type(obj) == "table" then
        if obj.is_player then
            return "have"
        end
    end

    return "has"
end

function en_US.does(obj)
    local n = obj
    if type(obj) == "table" then
        if obj.amount then
            n = obj.amount
        elseif obj.is_player then
            return "do"
        end
    end

    if n == 1 then
        return "do"
    end

    return "does"
end

function en_US.he(obj, ignore_sight)
    if type(obj) ~= "table" then
        return "it"
    end

    if ignore_sight then
        if obj.gender == "male" then
            return "he"
        elseif obj.gender == "female" then
            return "she"
        else
            return "it"
        end
    end

    if not obj.is_visible then
        return "it"
    end

    if obj.is_player then
        return "you"
    elseif obj.gender == "male" then
        return "he"
    elseif obj.gender == "female" then
        return "she"
    end

    return "it"
end

function en_US.his(obj, ignore_sight)
    if type(obj) ~= "table" then
        return "its"
    end

    if ignore_sight then
        if obj.gender == "male" then
            return "his"
        elseif obj.gender == "female" then
            return "hers"
        else
            return "its"
        end
    end

    if not obj.is_visible then
        return "its"
    end

    if obj.is_player then
        return "your"
    elseif obj.gender == "male" then
        return "his"
    elseif obj.gender == "female" then
        return "her"
    end

    return "its"
end

function en_US.him(obj, ignore_sight)
    if type(obj) ~= "table" then
        return "it"
    end

    if ignore_sight then
        if obj.gender == "male" then
            return "him"
        elseif obj.gender == "female" then
            return "her"
        else
            return "it"
        end
    end

    if not obj.is_visible then
        return "it"
    end

    if obj.is_player then
        return "you"
    elseif obj.gender == "male" then
        return "him"
    elseif obj.gender == "female" then
        return "her"
    end

    return "it"
end

function en_US.his_owned(obj)
    if type(obj) == "table" then
        if obj.is_player then
            return "r"
        end
    end

    return "'s"
end

function en_US.himself(obj)
    if type(obj) == "table" then
        if not obj.is_visible then
            return "itself"
        end
        if obj.is_player then
            return "yourself"
        elseif obj.gender == "male" then
            return "himself"
        elseif obj.gender == "female" then
            return "herself"
        end
    end

    return "itself"
end

function en_US.trim_job(name_with_job)
    return string.gsub(name_with_job, " .*", "") .. " "
end

function en_US.name_nojob(obj)
    return en_US.trim_job(en_US.name(obj))
end

function en_US.capitalize(str)
    if str == "" then
        return str
    end

    return str:gsub("^%l", string.upper)
end

function en_US.plural(count, suffix)
    suffix = suffix or "s"
    return count == 1 and "" or suffix
end

return en_US
